using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.Service
{
	/// <summary>
	/// Reads customer configuration file
	/// </summary>
	internal class ConfigurationReader
	{
			// By default, don't notify on file change
			private const bool DEFAULT_NOTIFY_BEHAVIOUR = false;

			#region Fields

			// The configuration file name
			private readonly string _configFileName;

			/// <summary>
			/// Raises when the configuraiton file is modified
			/// </summary>
			public event System.EventHandler FileChanged;

			#endregion Fields

			#region Constructor

			/// <summary>
			/// Initialize a new instance of the CustomConfigurationFileReader class that notifies 
			/// when the configuration file changes.
			/// </summary>
			/// <param name="configFileName">The full path to the custom configuration file</param>
			public ConfigurationReader(string configFileName)
				: this(configFileName, DEFAULT_NOTIFY_BEHAVIOUR)
			{            
			}        

			/// <summary>
			/// Initialize a new instance of the CustomConfigurationFileReader class
			/// </summary>
			/// <param name="configFileName">The full path to the custom configuration file</param>
			/// <param name="notifyOnFileChange">Indicate if to raise the FileChange event when the configuraiton file changes</param>
			public ConfigurationReader(string configFileName, bool notifyOnFileChange)
			{
				// Set the configuration file name
				_configFileName = configFileName;

				// Read the configuration File
				ReadConfiguration();

				// Start watch the configuration file (if notifyOnFileChanged is true)
				if(notifyOnFileChange)
					WatchConfigFile();
			}

			#endregion Constructor        

			/// <summary>
			/// Get the configuration that represents the content of the configuration file
			/// </summary>
			public System.Configuration.Configuration Config
			{
				get;
				set;
			}

			private System.Configuration.AppSettingsSection AppSettingsSection
			{
				get;
				set;
			}
			
		#region Helper Methods

			/// <summary>
			/// Watch the configuraiton file for changes
			/// </summary>
			private void WatchConfigFile()
			{
				var watcher = new FileSystemWatcher(_configFileName);
				watcher.Changed += ConfigFileChangedEvent;
			}

			/// <summary>
			/// Read the configuration file
			/// </summary>
			public void ReadConfiguration()
			{
				// Create config file map to point to the configuration file
				var configFileMap = new ExeConfigurationFileMap
				{
					ExeConfigFilename = _configFileName
				};

				// Create configuration object that contains the content of the custom configuration file
				Config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
				this.AppSettingsSection = (AppSettingsSection)Config.GetSection("appSettings");
			}        

			public string AppSetting(string key)
			{
				return AppSettingsSection.Settings[key].Value;
			}

			/// <summary>
			/// Called when the configuration file changed.
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			private void ConfigFileChangedEvent(object sender, FileSystemEventArgs e)
			{
				// Check if the file changed event has listeners
				if (FileChanged != null)
					// Raise the event
					FileChanged(this, new EventArgs());
			}

			#endregion Helper Methods
		}
}
