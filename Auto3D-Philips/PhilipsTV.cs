using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.ProcessPlugins.Auto3D;
using MediaPortal.Profile;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using System.Reflection;
using MediaPortal.Configuration;

namespace MediaPortal.ProcessPlugins.Auto3D.Devices
{
  public enum eConnectionMethod { jointSpaceV1, jointSpaceV5, DirectFB };

  public class PhilipsTV : Auto3DBaseDevice
  {
    eConnectionMethod _connectionMethod = eConnectionMethod.jointSpaceV1;
    private IPhilipsTVAdapter _currentAdapter;
    private static readonly IPhilipsTVAdapter _divineAdapter = new DiVineAdapter();
    private static readonly IPhilipsTVAdapter _jointSpaceV1Adapter = new JointSpaceV1Adapter();
    private static readonly IPhilipsTVAdapter _jointSpaceV5Adapter = new JointSpaceV5Adapter();

    public PhilipsTV()
    {
    }

    public override String CompanyName
    {
      get { return "Philips"; }
    }

    public override String DeviceName
    {
      get { return "Philips TV"; }
    }

    public String IPAddress
    {
      get;
      set;
    }

    public SystemBase Test()
    {
        if (_currentAdapter != null)
        {
            try
            {
                return _currentAdapter.Connect(IPAddress);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    public override void Start()
    {
      if (_currentAdapter != null)
      {
        _currentAdapter.Connect(IPAddress);
      }
    }

    public override void Stop()
    {
      if (_currentAdapter != null)
      {
        _currentAdapter.Disconnect();
      }
    }

    public eConnectionMethod ConnectionMethod
    {
      get { return _connectionMethod; }
      set
      {
        if (value != _connectionMethod)
        {
          Stop();
          switch (value)
          {
            case eConnectionMethod.DirectFB:
              _currentAdapter = _divineAdapter;
              break;
            case eConnectionMethod.jointSpaceV1:
              _currentAdapter = _jointSpaceV1Adapter;
              break;
            case eConnectionMethod.jointSpaceV5:
              _currentAdapter = _jointSpaceV5Adapter;
              break;
          }

          _connectionMethod = value;
        }
      }
    }

    public override void LoadSettings()
    {
      using (Settings reader = new MPSettings())
      {
        DeviceModelName = reader.GetValueAsString("Auto3DPlugin", CompanyName + "Model", "55PFL7606K-02");
        IPAddress = reader.GetValueAsString("Auto3DPlugin", "PhilipsAddress", "0.0.0.0");
        ConnectionMethod = (eConnectionMethod)reader.GetValueAsInt("Auto3DPlugin", "PhilipsConnectionMethod", (int)eConnectionMethod.jointSpaceV1);
      }
    }

    public override void SaveSettings()
    {
      using (Settings writer = new MPSettings())
      {
        writer.SetValue("Auto3DPlugin", "PhilipsModel", SelectedDeviceModel.Name);
        writer.SetValue("Auto3DPlugin", "PhilipsAddress", IPAddress);
        writer.SetValue("Auto3DPlugin", "PhilipsConnectionMethod", (int)ConnectionMethod);
      }
    }

    public override bool SendCommand(string command)
    {
      if (_currentAdapter != null)
      {
        return _currentAdapter.SendCommand(command);
      }

      return false;
    }

    public override bool CanTurnOff()
    {
        return true;
    }
  }
}
