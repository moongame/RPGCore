﻿using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using ContentCreator.Models;
using ElectronNET.API.Entities;
using ElectronNET.API;
using System.Linq;
using System.Threading.Tasks;
using System;
using ContentCreator.FileEditor;
using System.Collections.Generic;

namespace ContentCreator.Controllers
{
    public class StaticController : Controller
    {
		public List<FileManager> FileManagers = new List<FileManager>();

        public IActionResult Index()
        {
			var trayMenu = new MenuItem[] {
			new MenuItem
			{
				Label = "Create Contact",
				Click = () => Electron .WindowManager.BrowserWindows
					.First()
					.LoadURL($"http://localhost:{BridgeSettings.WebPort}/Contacts/Create")
				},
				new MenuItem
				{
				  Label = "Remove",
				  Click = () => Electron.Tray.Destroy()
				}
			};
			//Electron.Tray.Show("/Assets/GraphIcon Large.png", trayMenu);
			
			var windowMenu = new MenuItem[] {
				new MenuItem {
					Label = "File", Submenu = new MenuItem[] {
						new MenuItem
						{
							Label = "Open",
							Accelerator = "CmdOrCtrl+O",
							Click = async () =>
							{
								var mainWindow = Electron.WindowManager.BrowserWindows.First();

								Electron.IpcMain.Send(mainWindow, "NEW_DOCUMENT_NEEDED", "Create new document");

								/*var saveOptions = new SaveDialogOptions()
								{
									ButtonLabel = "Save",
									Filters = new FileFilter[]
									{
										new FileFilter { Extensions = new string[] { ".bhvr" } }
									},
									NameFieldLabel = "Behaviour"
								};
								string[] result = await Electron.Dialog.ShowSaveDialogAsync(mainWindow, saveOptions);*/
								
								var openDialog = new OpenDialogOptions()
								{
									ButtonLabel = "Open",
									Filters = new FileFilter[]
									{
										new FileFilter { Extensions = new string[] { ".bhvr" } }
									}
								};
								string[] result = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, openDialog);
								
								var manager = new FileManager(result[0]);
								FileManagers.Add(manager);
							}
						},
						new MenuItem
						{
							Label = "New",
							Accelerator = "CmdOrCtrl+N",
							Click = async () =>
							{
								var mainWindow = Electron.WindowManager.BrowserWindows.First();

								Electron.IpcMain.Send(mainWindow, "NEW_DOCUMENT_NEEDED", "Create new document");

								var saveOptions = new SaveDialogOptions()
								{
									ButtonLabel = "Save",
									Filters = new FileFilter[]
									{
										new FileFilter { Extensions = new string[] { ".bhvr" } }
									},
									NameFieldLabel = "Behaviour"
								};

								var result = await Electron.Dialog.ShowSaveDialogAsync(mainWindow, saveOptions);

								string[] data = System.IO.File.ReadAllLines(result);

								Console.WriteLine(string.Join('\n', data));
							}
						},
					}
				},
				new MenuItem {
					Label = "Edit", Submenu = new MenuItem[] {
						new MenuItem { Label = "Undo", Accelerator = "CmdOrCtrl+Z", Role = MenuRole.undo },
						new MenuItem { Label = "Redo", Accelerator = "Shift+CmdOrCtrl+Z", Role = MenuRole.redo },
						new MenuItem { Type = MenuType.separator },
						new MenuItem { Label = "Cut", Accelerator = "CmdOrCtrl+X", Role = MenuRole.cut },
						new MenuItem { Label = "Copy", Accelerator = "CmdOrCtrl+C", Role = MenuRole.copy },
						new MenuItem { Label = "Paste", Accelerator = "CmdOrCtrl+V", Role = MenuRole.paste },
						new MenuItem { Label = "Select All", Accelerator = "CmdOrCtrl+A", Role = MenuRole.selectall }
					}
				},
				new MenuItem {
					Label = "View", Submenu = new MenuItem[]
					{
						new MenuItem
						{
							Label = "Reload",
							Accelerator = "CmdOrCtrl+R",
							Click = () =>
							{
								// on reload, start fresh and close any old
								// open secondary windows
								Electron.WindowManager.BrowserWindows.ToList().ForEach(browserWindow => {
									if(browserWindow.Id != 1)
									{
										browserWindow.Close();
									}
									else
									{
										browserWindow.Reload();
									}
								});
							}
						},
						new MenuItem
						{
							Label = "Toggle Full Screen",
							Accelerator = "CmdOrCtrl+F",
							Click = async () =>
							{
								bool isFullScreen = await Electron.WindowManager.BrowserWindows.First().IsFullScreenAsync();
								Electron.WindowManager.BrowserWindows.First().SetFullScreen(!isFullScreen);
							}
						},
						new MenuItem
						{
							Label = "Open Developer Tools",
							Accelerator = "CmdOrCtrl+I",
							Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
						},
						new MenuItem
						{
							Type = MenuType.separator
						},
						new MenuItem
						{
							Label = "App Menu Demo",
							Click = async () => {
								var options = new MessageBoxOptions("This demo is for the Menu section, showing how to create a clickable menu item in the application menu.");
								options.Type = MessageBoxType.info;
								options.Title = "Application Menu Demo";
								await Electron.Dialog.ShowMessageBoxAsync(options);
							}
						}
					}
				},
				new MenuItem {
					Label = "Window", Role = MenuRole.window, Submenu = new MenuItem[] {
						new MenuItem { Label = "Minimize", Accelerator = "CmdOrCtrl+M", Role = MenuRole.minimize },
						new MenuItem { Label = "Close", Accelerator = "CmdOrCtrl+W", Role = MenuRole.close }
					}
				}
			};
			Electron.Menu.SetApplicationMenu(windowMenu);
			
			return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
