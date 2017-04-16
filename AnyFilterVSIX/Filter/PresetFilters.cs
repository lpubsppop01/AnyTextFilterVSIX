﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace lpubsppop01.AnyFilterVSIX
{
    enum PresetFilterID
    {
        Empty, Sed, Awk, MonoCSharpScript, CygwinBash, CygwinSed, CygwinGawk
    }

    class PresetFilters
    {
        public static Filter Get(PresetFilterID presetID)
        {
            var filter = new Filter();
            switch (presetID)
            {
                default:
                case PresetFilterID.Empty:
                    filter.Title = "Empty";
                    break;
                case PresetFilterID.Sed:
                    filter.Title = "sed";
                    filter.Command = "sed.exe";
                    filter.Arguments = string.Format(@"-f ""{0}""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.Awk:
                    filter.Title = "AWK";
                    filter.Command = "awk.exe";
                    filter.Arguments = string.Format(@"-f ""{0}""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.MonoCSharpScript:
                    filter.Title = "Mono C# Script";
                    filter.Command = "cmd";
                    filter.Arguments = string.Format(@"/c ""C:\Program Files (x86)\Mono\bin\csharp.bat"" {0}", FilterRunner.VariableName_InputTempFilePath);
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.CurrentLine;
                    filter.InsertsAfterTargetSpan = true;
                    break;
                case PresetFilterID.CygwinBash:
                    filter.Title = "Cygwin bash";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    filter.Arguments = string.Format(@"-lc ""$(cygpath -u '{0}')""", FilterRunner.VariableName_InputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.TemplateFilePath = Path.Combine(GetTemplateDirectoryPath(), "CygwinBashTemplate.txt");
                    filter.UsesTemplateFile = true;
                    break;
                case PresetFilterID.CygwinSed:
                    filter.Title = "Cygwin sed";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "sed -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""sed -f \""$(cygpath -u '{0}')\""""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
                case PresetFilterID.CygwinGawk:
                    filter.Title = "Cygwin Gawk";
                    filter.Command = @"C:\cygwin64\bin\bash.exe";
                    // unescaped: -lc "awk -f \"$(cygpath -u '$(UserInputTempFilePath)')\""
                    filter.Arguments = string.Format(@"-lc ""awk -f \""$(cygpath -u '{0}')\""""", FilterRunner.VariableName_UserInputTempFilePath);
                    filter.InputNewLineKind = NewLineKind.LF;
                    filter.InputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.OutputEncodingName = MyEncodingInfo.UTF8_WithoutBOM.Name;
                    filter.TargetSpanForNoSelection = TargetSpanForNoSelection.WholeDocument;
                    filter.PassesInputTextToStandardInput = true;
                    break;
            }
            return filter;
        }

        static string GetTemplateDirectoryPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AnyFilterVSIX");
        }

        public static bool TryCreateTemplateFile(PresetFilterID presetID, string filePath, string encodingName, NewLineKind newLineKind)
        {
            try
            {
                string dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                switch (presetID)
                {
                    case PresetFilterID.CygwinBash:
                        using (var writer = new StreamWriter(filePath, /* append */ false, MyEncoding.GetEncoding(encodingName))
                        {
                            NewLine = newLineKind.ToNewLineString()
                        })
                        {
                            writer.WriteLine(@"INPUT_TEXT=$(cat << 'EOS'");
                            writer.WriteLine(@"$(InputText)");
                            writer.WriteLine(@"EOS");
                            writer.WriteLine(@")");
                            writer.WriteLine(@"echo ""$INPUT_TEXT"" | $(UserInput)");
                        }
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
