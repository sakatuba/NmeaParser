﻿using Esri.ArcGISRuntime.Geometry;
using System;

namespace SampleApp.WinDesktop
{
	public class NmeaLocationProvider : Esri.ArcGISRuntime.Location.ILocationProvider
	{
		public event EventHandler<Esri.ArcGISRuntime.Location.LocationInfo> LocationChanged;
		private NmeaParser.NmeaDevice device;
		double m_Accuracy = double.NaN;

		public NmeaLocationProvider(NmeaParser.NmeaDevice device)
		{
			this.device = device;
			device.MessageReceived += device_MessageReceived;
		}

		private void device_MessageReceived(NmeaParser.NmeaDevice sender, NmeaParser.Nmea.NmeaMessage args)
		{
			if (args is NmeaParser.Nmea.Gps.Garmin.Pgrme)
			{
				m_Accuracy = ((NmeaParser.Nmea.Gps.Garmin.Pgrme)args).HorizontalError;
			}
			else if (args is NmeaParser.Nmea.Gps.Gprmc)
			{
				var rmc = (NmeaParser.Nmea.Gps.Gprmc)args;
				if(rmc.Active && LocationChanged != null)
				{
					LocationChanged(this, new Esri.ArcGISRuntime.Location.LocationInfo()
					{
						Course = rmc.Course,
						Speed = rmc.Speed,
						HorizontalAccuracy = m_Accuracy,
						Location = new Esri.ArcGISRuntime.Geometry.MapPoint(rmc.Longitude, rmc.Latitude, SpatialReferences.Wgs84)
					});
				}
			}
		}

		public System.Threading.Tasks.Task StartAsync()
		{
			return this.device.OpenAsync();
		}

		public System.Threading.Tasks.Task StopAsync()
		{
			return this.device.CloseAsync();
			m_Accuracy = double.NaN;
		}
	}
}
