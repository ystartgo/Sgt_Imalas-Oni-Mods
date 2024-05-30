﻿using BlueprintsV2.BlueprintsV2.BlueprintData;
using HarmonyLib;
using PeterHan.PLib.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UtilLibs;
using static BlueprintsV2.STRINGS;

namespace BlueprintsV2
{
    internal class ModAssets
    {

        public static Sprite BLUEPRINTS_CREATE_ICON_SPRITE;
        public static Sprite BLUEPRINTS_CREATE_VISUALIZER_SPRITE;

        public static Sprite BLUEPRINTS_USE_ICON_SPRITE;
        public static Sprite BLUEPRINTS_USE_VISUALIZER_SPRITE;

        public static Sprite BLUEPRINTS_SNAPSHOT_ICON_SPRITE;
        public static Sprite BLUEPRINTS_SNAPSHOT_VISUALIZER_SPRITE;

        public static Color BLUEPRINTS_COLOR_VALIDPLACEMENT = Color.white;
        public static Color BLUEPRINTS_COLOR_INVALIDPLACEMENT = Color.red;
        public static Color BLUEPRINTS_COLOR_NOTECH = new Color32(30, 144, 255, 255);
        public static Color BLUEPRINTS_COLOR_BLUEPRINT_DRAG = new Color32(0, 119, 145, 255);

        public static HashSet<char> BLUEPRINTS_FILE_DISALLOWEDCHARACTERS;
        public static HashSet<char> BLUEPRINTS_PATH_DISALLOWEDCHARACTERS;

        public static HashSet<string> BLUEPRINTS_AUTOFILE_IGNORE = new();
        public static FileSystemWatcher BLUEPRINTS_AUTOFILE_WATCHER;

        static ModAssets()
        {
            BLUEPRINTS_FILE_DISALLOWEDCHARACTERS = new HashSet<char>();
            BLUEPRINTS_FILE_DISALLOWEDCHARACTERS.UnionWith(System.IO.Path.GetInvalidFileNameChars());

            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS = new HashSet<char>();
            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS.UnionWith(Path.GetInvalidFileNameChars());
            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS.UnionWith(Path.GetInvalidPathChars());

            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS.Remove('/');
            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS.Remove('\\');
            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS.Remove(Path.DirectorySeparatorChar);
            BLUEPRINTS_PATH_DISALLOWEDCHARACTERS.Remove(Path.AltDirectorySeparatorChar);

        }

        public static GameObject BlueprintSelectionScreen;
        public static void LoadAssets()
        {
            var bundle = AssetUtils.LoadAssetBundle("blueprints_ui", platformSpecific: true);
            BlueprintSelectionScreen = bundle.LoadAsset<GameObject>("Assets/UIs/blueprintSelector.prefab");

            //UIUtils.ListAllChildren(Assets.transform);

            var TMPConverter = new TMPConverter();
            TMPConverter.ReplaceAllText(BlueprintSelectionScreen);
        }

        public static class BlueprintFileHandling
        {
            public static string GetBlueprintDirectory()
            {
                string folderLocation = Path.Combine(Util.RootFolder(), "blueprints");
                if (!Directory.Exists(folderLocation))
                {
                    Directory.CreateDirectory(folderLocation);
                }

                return folderLocation;
            }

            public static bool AttachFileWatcher()
            {
                string blueprintDirectory = GetBlueprintDirectory();

                ModAssets.BLUEPRINTS_AUTOFILE_WATCHER = new FileSystemWatcher
                {
                    Path = blueprintDirectory,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                    Filter = "*.*"
                };

                ModAssets.BLUEPRINTS_AUTOFILE_WATCHER.Created += (sender, eventArgs) =>
                {
                    if (ModAssets.BLUEPRINTS_AUTOFILE_IGNORE.Contains(eventArgs.FullPath))
                    {
                        ModAssets.BLUEPRINTS_AUTOFILE_IGNORE.Remove(eventArgs.FullPath);
                        return;
                    }

                    if (eventArgs.FullPath.EndsWith(".blueprint") || eventArgs.FullPath.EndsWith(".json"))
                    {
                        if (LoadBlueprint(eventArgs.FullPath, out Blueprint blueprint))
                        {
                            PlaceIntoFolder(blueprint);
                        }
                    }
                };

                ModAssets.BLUEPRINTS_AUTOFILE_WATCHER.EnableRaisingEvents = true;
                return false;
            }

            public static void ReloadBlueprints(bool ingame)
            {
                BlueprintsState.LoadedBlueprints.Clear();
                LoadFolder(GetBlueprintDirectory());

                if (ingame && BlueprintsState.HasBlueprints())
                {
                    BlueprintsState.ClearVisuals();
                    BlueprintsState.VisualizeBlueprint(Grid.PosToXY(PlayerController.GetCursorPos(KInputManager.GetMousePos())), BlueprintsState.SelectedBlueprint);
                }
            }

            public static void LoadFolder(string folder)
            {
                string[] files = Directory.GetFiles(folder);
                string[] subfolders = Directory.GetDirectories(folder);

                foreach (string file in files)
                {
                    if (file.EndsWith(".blueprint") || file.EndsWith(".json"))
                    {
                        if (LoadBlueprint(file, out Blueprint blueprint))
                        {
                            PlaceIntoFolder(blueprint);
                        }
                    }
                }

                foreach (string subfolder in subfolders)
                {
                    LoadFolder(subfolder);
                }
            }

            public static bool LoadBlueprint(string blueprintLocation, out Blueprint blueprint)
            {
                blueprint = new Blueprint(blueprintLocation);
                if (!blueprint.ReadBinary())
                {
                    blueprint.ReadJson();
                }

                return !blueprint.IsEmpty();
            }

            public static void PlaceIntoFolder(Blueprint blueprint)
            {
                int index = -1;

                for (int i = 0; i < BlueprintsState.LoadedBlueprints.Count; ++i)
                {
                    if (BlueprintsState.LoadedBlueprints[i].Name == blueprint.Folder)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    BlueprintFolder newFolder = new BlueprintFolder(blueprint.Folder);
                    newFolder.AddBlueprint(blueprint);

                    BlueprintsState.LoadedBlueprints.Add(newFolder);
                }

                else
                {
                    BlueprintsState.LoadedBlueprints[index].AddBlueprint(blueprint);
                }
            }
        }
        public static class DialogHandling
        {
            public static FileNameDialog CreateTextDialog(string title, bool allowEmpty = false, System.Action<string, FileNameDialog> onConfirm = null)
            {
                GameObject textDialogParent = GameScreenManager.Instance.GetParent(GameScreenManager.UIRenderTarget.ScreenSpaceOverlay);
                FileNameDialog textDialog = Util.KInstantiateUI<FileNameDialog>(ScreenPrefabs.Instance.FileNameDialog.gameObject, textDialogParent);
                textDialog.name = "BlueprintsMod_TextDialog_" + title;

                TMP_InputField inputField = Traverse.Create(textDialog).Field("inputField").GetValue<TMP_InputField>();
                KButton confirmButton = Traverse.Create(textDialog).Field("confirmButton").GetValue<KButton>();
                if (inputField != null && confirmButton && confirmButton != null && allowEmpty)
                {
                    confirmButton.onClick += delegate
                    {
                        if (textDialog.onConfirm != null && inputField.text != null && inputField.text.Length == 0)
                        {
                            textDialog.onConfirm.Invoke(inputField.text);
                        }
                    };
                }

                if (onConfirm != null)
                {
                    textDialog.onConfirm += delegate (string result)
                    {
                        onConfirm.Invoke(result.Substring(0, Mathf.Max(0, result.Length - 4)), textDialog);
                    };
                }

                Transform titleTransform = textDialog.transform.Find("Panel")?.Find("Title_BG")?.Find("Title");
                if (titleTransform != null && titleTransform.GetComponent<LocText>() != null)
                {
                    titleTransform.GetComponent<LocText>().text = title;
                }

                return textDialog;
            }

            public static FileNameDialog CreateFolderDialog(System.Action<string, FileNameDialog> onConfirm = null)
            {
                string title = STRINGS.UI.TOOLS.FOLDERBLUEPRINT_TITLE;

                FileNameDialog folderDialog = CreateTextDialog(title, true, onConfirm);
                folderDialog.name = "BlueprintsMod_FolderDialog_" + title;

                return folderDialog;
            }
        }

        internal static void RegisterActions()
        {
            Actions.BlueprintsCreateAction = new PActionManager().CreateAction(ActionKeys.ACTION_CREATE_KEY,
                STRINGS.UI.ACTIONS.CREATE_TITLE, new PKeyBinding());
            Actions.BlueprintsUseAction = new PActionManager().CreateAction(ActionKeys.ACTION_USE_KEY,
                STRINGS.UI.ACTIONS.USE_TITLE, new PKeyBinding());
            Actions.BlueprintsCreateFolderAction = new PActionManager().CreateAction(ActionKeys.ACTION_CREATEFOLDER_KEY,
                STRINGS.UI.ACTIONS.CREATEFOLDER_TITLE, new PKeyBinding(KKeyCode.Home));
            Actions.BlueprintsRenameAction = new PActionManager().CreateAction(ActionKeys.ACTION_RENAME_KEY,
                STRINGS.UI.ACTIONS.RENAME_TITLE, new PKeyBinding(KKeyCode.End));
            Actions.BlueprintsCycleFoldersNextAction = new PActionManager().CreateAction(ActionKeys.ACTION_CYCLEFOLDERS_NEXT_KEY,
                STRINGS.UI.ACTIONS.CYCLEFOLDERS_NEXT_TITLE, new PKeyBinding(KKeyCode.UpArrow));
            Actions.BlueprintsCycleFoldersPrevAction = new PActionManager().CreateAction(ActionKeys.ACTION_CYCLEFOLDERS_PREV_KEY,
                STRINGS.UI.ACTIONS.CYCLEFOLDERS_PREV_TITLE, new PKeyBinding(KKeyCode.DownArrow));
            Actions.BlueprintsCycleBlueprintsNextAction = new PActionManager().CreateAction(ActionKeys.ACTION_CYCLEBLUEPRINTS_NEXT_KEY,
                STRINGS.UI.ACTIONS.CYCLEBLUEPRINTS_NEXT_TITLE, new PKeyBinding(KKeyCode.RightArrow));
            Actions.BlueprintsCycleBlueprintsPrevAction = new PActionManager().CreateAction(ActionKeys.ACTION_CYCLEBLUEPRINTS_PREV_KEY,
                STRINGS.UI.ACTIONS.CYCLEBLUEPRINTS_PREV_TITLE, new PKeyBinding(KKeyCode.LeftArrow));
            Actions.BlueprintsSnapshotAction = new PActionManager().CreateAction(ActionKeys.ACTION_SNAPSHOT_KEY,
                STRINGS.UI.ACTIONS.SNAPSHOT_TITLE, new PKeyBinding());
            Actions.BlueprintsDeleteAction = new PActionManager().CreateAction(ActionKeys.ACTION_DELETE_KEY,
                STRINGS.UI.ACTIONS.DELETE_TITLE, new PKeyBinding(KKeyCode.Delete));
        }

        public static class ActionKeys
        {
            public static string ACTION_CREATE_KEY = "Blueprints.create.opentool";
            public static string ACTION_USE_KEY = "Blueprints.use.opentool";
            public static string ACTION_CREATEFOLDER_KEY = "Blueprints.use.assignfolder";
            public static string ACTION_RENAME_KEY = "Blueprints.use.rename";
            public static string ACTION_CYCLEFOLDERS_NEXT_KEY = "Blueprints.use.cyclefolders.next";
            public static string ACTION_CYCLEFOLDERS_PREV_KEY = "Blueprints.use.cyclefolders.previous";
            public static string ACTION_CYCLEBLUEPRINTS_NEXT_KEY = "Blueprints.use.cycleblueprints.next";
            public static string ACTION_CYCLEBLUEPRINTS_PREV_KEY = "Blueprints.use.cycleblueprints.previous";
            public static string ACTION_SNAPSHOT_KEY = "Blueprints.snapshot.opentool";
            public static string ACTION_DELETE_KEY = "Blueprints.multi.delete";
        }
        public static class Actions
        {
            public static PAction BlueprintsCreateAction { get; set; }
            public static PAction BlueprintsUseAction { get; set; }
            public static PAction BlueprintsCreateFolderAction { get; set; }
            public static PAction BlueprintsRenameAction { get; set; }
            public static PAction BlueprintsCycleFoldersNextAction { get; set; }
            public static PAction BlueprintsCycleFoldersPrevAction { get; set; }
            public static PAction BlueprintsCycleBlueprintsNextAction { get; set; }
            public static PAction BlueprintsCycleBlueprintsPrevAction { get; set; }
            public static PAction BlueprintsSnapshotAction { get; set; }
            public static PAction BlueprintsDeleteAction { get; set; }
        }
    }
}