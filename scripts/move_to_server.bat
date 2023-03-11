:: Move all plugins to local server
xcopy /s /y ..\GamingAPIPlugins\plugins ..\..\rust-docker-image\rust\oxide\plugins

:: Move extensions to local server
xcopy /s /y ..\GamingAPIPlugins\bin\local\net48\GamingAPIPlugins.dll ..\..\rust-docker-image\rust\RustDedicated_Data\Managed\*
xcopy /s /y ..\GamingAPIPlugins\bin\local\net48\NATS.Client.dll ..\..\rust-docker-image\rust\RustDedicated_Data\Managed\*
