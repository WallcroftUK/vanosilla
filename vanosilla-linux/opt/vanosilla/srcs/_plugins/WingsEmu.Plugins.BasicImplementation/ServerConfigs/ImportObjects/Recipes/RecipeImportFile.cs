﻿using System.Collections.Generic;
using WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using YamlDotNet.Serialization;

namespace WingsEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;

public class RecipeImportFile : IFileData
{
    [YamlMember(Alias = "recipes", ApplyNamingConventions = true)]
    public List<RecipeObject> Recipes { get; set; }
}