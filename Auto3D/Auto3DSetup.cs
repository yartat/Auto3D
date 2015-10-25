﻿
using System;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using System.Windows.Forms;
using MediaPortal.ProcessPlugins.Auto3D.Devices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using MediaPortal.ProcessPlugins.Auto3D.UPnP;

namespace MediaPortal.ProcessPlugins.Auto3D
{
  public partial class Auto3DSetup : MPConfigForm
  {
    [DllImport("user32.dll")]
    static extern bool HideCaret(IntPtr hWnd);

    IAuto3D _lastDevice = null;

    public Auto3DSetup(List<IAuto3D> list)
    {
      InitializeComponent();

      String labeInfoText = "";

      foreach (IAuto3D item in list)
      {
        comboBoxModel.Items.Add(item);
        panelSettings.Controls.Add(item.GetSetupControl());
        //item.GetSetupControl().Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        item.GetSetupControl().Dock = DockStyle.Fill;

        String[] versionInfo = item.GetType().Assembly.FullName.Split(',');
        String[] version = versionInfo[1].Split('=');
        labeInfoText += versionInfo[0] + "\n" + version[0] + " = " + version[1] + "\n\n";
      }

      String [] versionInfoUPnP = System.Reflection.Assembly.GetAssembly(typeof(MediaPortal.ProcessPlugins.Auto3D.UPnP.Auto3DUPnP)).FullName.Split(',');
      String[] versionUPnP = versionInfoUPnP[1].Split('=');

      labeInfoText += versionInfoUPnP[0] + " (UPnP)\n" + versionUPnP[0] + " = " + versionUPnP[1] + "\n\n";

      labelInfo.Text = labeInfoText;

      textBoxMenuHotkey.GotFocus += textBoxMenuHotkey_GotFocus;
      textBoxMenuHotkey.LostFocus += textBoxMenuHotkey_LostFocus;
      textBoxMenuHotkey.KeyDown += textBoxMenuHotkey_KeyDown;

      LoadSettings();
    }

    void textBoxMenuHotkey_KeyDown(object sender, KeyEventArgs e)
    {
      if ((int)e.KeyCode == 16 || (int)e.KeyCode == 17 || (int)e.KeyCode == 18)
      {
        textBoxMenuHotkey.Text = (e.Shift ? "Shift + " : "") + (e.Control ? "Ctrl + " : "") + (e.Alt ? "Alt + " : "");
        textBoxMenuHotkey.Text = textBoxMenuHotkey.Text.TrimEnd('+');
      }
      else
        textBoxMenuHotkey.Text = (e.Shift ? "Shift + " : "") + (e.Control ? "Ctrl + " : "") + (e.Alt ? "Alt + " : "") + e.KeyCode;
    }

    void textBoxMenuHotkey_LostFocus(object sender, EventArgs e)
    {
      textBoxMenuHotkey.BackColor = SystemColors.Info;
    }

    void textBoxMenuHotkey_GotFocus(object sender, EventArgs e)
    {
      HideCaret(textBoxMenuHotkey.Handle);
      textBoxMenuHotkey.BackColor = SystemColors.MenuHighlight;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);

      foreach (IAuto3D item in comboBoxModel.Items)
      {
        panelSettings.Controls.Remove(item.GetSetupControl());
      }

      comboBoxModel.Items.Clear();
    }

    public void LoadSettings()
    {
      using (Settings reader = new MPSettings())
      {
        checkBoxTV.Checked = reader.GetValueAsBool("Auto3DPlugin", "TV", false);
        checkBoxVideo.Checked = reader.GetValueAsBool("Auto3DPlugin", "Video", true);

        String activeDeviceName = reader.GetValueAsString("Auto3DPlugin", "ActiveDevice", "");

        comboBoxModel.SelectedIndex = 0;

        foreach (IAuto3D device in comboBoxModel.Items)
        {
          if (device.ToString() == activeDeviceName)
          {
            comboBoxModel.SelectedItem = device;
            break;
          }
        }

        checkBoxSelectionAlways.Checked = reader.GetValueAsBool("Auto3DPlugin", "3DMenuAlways", false);

        checkBoxName3DSimple.Checked = reader.GetValueAsBool("Auto3DPlugin", "CheckNameSimple", true);
        checkBoxName3DFull.Checked = reader.GetValueAsBool("Auto3DPlugin", "CheckNameFull", true);

        radioButtonSBS.Checked = reader.GetValueAsBool("Auto3DPlugin", "CheckNameFormatSBS", true);
        radioButtonTAB.Checked = !reader.GetValueAsBool("Auto3DPlugin", "CheckNameFormatSBS", true);

        checkBoxSBS.Checked = reader.GetValueAsBool("Auto3DPlugin", "SideBySide", true);
        checkBoxTopDown.Checked = reader.GetValueAsBool("Auto3DPlugin", "TopAndBottom", false);

        checkBoxSelectionOnKey.Checked = reader.GetValueAsBool("Auto3DPlugin", "3DMenuOnKey", false);
        textBoxMenuHotkey.Text = reader.GetValueAsString("Auto3DPlugin", "3DMenuKey", "CTRL + D");

        checkBoxEventGhost.Checked = reader.GetValueAsBool("Auto3DPlugin", "EventGhostEvents", false);

        checkBox3DSubTitles.Checked = reader.GetValueAsBool("Auto3DPlugin", "3DSubtitles", true);
        trackBarDepth3D.Value = reader.GetValueAsInt("Auto3DPlugin", "SubtitleDepth", 0);

        checkBoxModeSwitchMessage.Checked = reader.GetValueAsBool("Auto3DPlugin", "ShowMessageOnModeChange", true);
        checkBoxSupressSwitchBackTo2D.Checked = reader.GetValueAsBool("Auto3DPlugin", "SupressSwitchBackTo2D", false);
        checkBoxConvert3Dto2D.Checked = reader.GetValueAsBool("Auto3DPlugin", "Convert3DTo2D", false);

        textBoxSBS.Text = reader.GetValueAsString("Auto3DPlugin", "SwitchSBSLabels", "\"3DSBS\", \"3D SBS\"");
        textBoxSBSR.Text = reader.GetValueAsString("Auto3DPlugin", "SwitchSBSRLabels", "\"3DSBSR\", \"3D SBS R\"");
        textBoxTAB.Text = reader.GetValueAsString("Auto3DPlugin", "SwitchTABLabels", "\"3DTAB\", \"3D TAB\"");
        textBoxTABR.Text = reader.GetValueAsString("Auto3DPlugin", "SwitchTABRLabels", "\"3DTABR\", \"3D TAB R\"");

        checkBoxLogKnown.Checked = reader.GetValueAsBool("Auto3DPlugin", "LogOnlyKnownDevices", true);
        checkBoxPreRendered.Checked = reader.GetValueAsBool("Auto3DPlugin", "StretchSubtitles", false);

        checkBoxTurnOffDevice.Checked = reader.GetValueAsBool("Auto3DPlugin", "TurnDeviceOff", false);
        comboBoxTurnOffDevice.SelectedIndex = reader.GetValueAsInt("Auto3DPlugin", "TurnDeviceOffWhen", 0);
      }

      foreach (IAuto3D item in comboBoxModel.Items)
      {
        IAuto3DSetup setup = (IAuto3DSetup)item.GetSetupControl();
        setup.LoadSettings();
      }
    }

    public void SaveSettings()
    {
      using (Settings writer = new MPSettings())
      {
        writer.SetValueAsBool("Auto3DPlugin", "TV", checkBoxTV.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "Video", checkBoxVideo.Checked);
        writer.SetValue("Auto3DPlugin", "ActiveDevice", comboBoxModel.SelectedItem.ToString());

        writer.SetValueAsBool("Auto3DPlugin", "3DMenuAlways", checkBoxSelectionAlways.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "CheckNameSimple", checkBoxName3DSimple.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "CheckNameFull", checkBoxName3DFull.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "CheckNameFormatSBS", radioButtonSBS.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "SideBySide", checkBoxSBS.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "TopAndBottom", checkBoxTopDown.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "3DMenuOnKey", checkBoxSelectionOnKey.Checked);
        writer.SetValue("Auto3DPlugin", "3DMenuKey", textBoxMenuHotkey.Text);

        writer.SetValueAsBool("Auto3DPlugin", "EventGhostEvents", checkBoxEventGhost.Checked);

        writer.SetValueAsBool("Auto3DPlugin", "ShowMessageOnModeChange", checkBoxModeSwitchMessage.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "SupressSwitchBackTo2D", checkBoxSupressSwitchBackTo2D.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "Convert3DTo2D", checkBoxConvert3Dto2D.Checked);

        writer.SetValueAsBool("Auto3DPlugin", "3DSubtitles", checkBox3DSubTitles.Checked);
        writer.SetValue("Auto3DPlugin", "SubtitleDepth", trackBarDepth3D.Value);

        writer.SetValue("Auto3DPlugin", "SwitchSBSLabels", textBoxSBS.Text);
        writer.SetValue("Auto3DPlugin", "SwitchSBSRLabels", textBoxSBSR.Text);
        writer.SetValue("Auto3DPlugin", "SwitchTABLabels", textBoxTAB.Text);
        writer.SetValue("Auto3DPlugin", "SwitchTABRLabels", textBoxTABR.Text);

        writer.SetValueAsBool("Auto3DPlugin", "LogOnlyKnownDevices", checkBoxLogKnown.Checked);
        writer.SetValueAsBool("Auto3DPlugin", "StretchSubtitles", checkBoxPreRendered.Checked);

        if (checkBoxTurnOffDevice.Enabled)
        {
            writer.SetValueAsBool("Auto3DPlugin", "TurnDeviceOff", checkBoxTurnOffDevice.Checked);
            writer.SetValue("Auto3DPlugin", "TurnDeviceOffWhen", comboBoxTurnOffDevice.SelectedIndex);
        }
      }

      foreach (IAuto3D item in comboBoxModel.Items)
      {
        IAuto3DSetup setup = (IAuto3DSetup)item.GetSetupControl();
        setup.SaveSettings();
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      SaveSettings();

      foreach (IAuto3D item in comboBoxModel.Items)
        panelSettings.Controls.Remove(item.GetSetupControl());

      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      foreach (IAuto3D item in comboBoxModel.Items)
        panelSettings.Controls.Remove(item.GetSetupControl());

      Close();
    }

    private void comboBoxModel_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_lastDevice != null)
        _lastDevice.Stop();

      Auto3DUPnP.StopSSDP();

      IAuto3DSetup setup = (IAuto3DSetup)((IAuto3D)comboBoxModel.SelectedItem).GetSetupControl();
      setup.BringToFront();
      setup.GetDevice().Start();

      if (setup.GetDevice() is Auto3DUPnPBaseDevice)
        Auto3DUPnP.StartSSDP();

      buttonConfig.Visible = (setup.GetDevice().GetRemoteControl() != null);
      
      checkBoxTurnOffDevice.Enabled = setup.GetDevice().CanTurnOff();
      buttonTurnOffDevice.Enabled = setup.GetDevice().CanTurnOff();

      if (!setup.GetDevice().CanTurnOff())
        checkBoxTurnOffDevice.Checked = false;      
    }

    private void radioButtonSBS_CheckedChanged(object sender, EventArgs e)
    {
      radioButtonTAB.Checked = !radioButtonSBS.Checked;
    }

    private void checkBoxNameCheck_CheckedChanged(object sender, EventArgs e)
    {
      radioButtonSBS.Enabled = checkBoxName3DSimple.Checked;
      radioButtonTAB.Enabled = checkBoxName3DSimple.Checked;
    }

    private void checkBoxSelectionAlways_CheckedChanged(object sender, EventArgs e)
    {
      panel3D.Enabled = !checkBoxSelectionAlways.Checked;
    }

    private void buttonConfig_Click(object sender, EventArgs e)
    {
      IAuto3DSetup setup = (IAuto3DSetup)((IAuto3D)comboBoxModel.SelectedItem).GetSetupControl();
      setup.SaveSettings();
      setup.GetDevice().LoadSettings();

      Auto3DSequenceManager sequenceManager = new Auto3DSequenceManager();
      sequenceManager.SetDevice(setup.GetDevice());
      Auto3DSequenceManager.SequenceManager = sequenceManager;
      sequenceManager.ShowDialog();

      setup.LoadSettings();
    }

    private void linkLabelAuto3D_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      linkLabelAuto3D.LinkVisited = true;
      System.Diagnostics.Process.Start("http://forum.team-mediaportal.com/threads/auto3d-plugin-for-mediaportal-1-2-3-and-1-3-0-final.116708/");
    }

    private void buttonTurnOff_Click(object sender, EventArgs e)
    {
        ((IAuto3D)comboBoxModel.SelectedItem).TurnOff();
    }
  }
}
