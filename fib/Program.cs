using Microsoft.VisualBasic.FileIO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;

namespace fib
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool isValid = false;

            //bundle command
            var bundleCommand = new Command("bundle", "Bundle code files to a single file");

            //the options
            var rspOptionOutput = new Option<string>("--output", "file path");
            var bundleOptionOutput = new Option<FileInfo>("--output", "file path and name");
            var bundleOptionLanguage = new Option<string[]>("--language", "selected programming language")
            {
                IsRequired = true

            };
            var bundleOptionNote = new Option<bool>("--note", "include directory and name of code file");
            var bundleOptionSort = new Option<string>("--sort", "sort the writing of codefiles by order");
            var bundleOptionDeleteEmptyLines = new Option<bool>("--remove", "delete empty lines from code");
            var bundleOptionAuthor = new Option<string>("--author", "note the file's author");

            //aliases
            bundleOptionAuthor.AddAlias("-a");
            bundleOptionDeleteEmptyLines.AddAlias("-d");
            bundleOptionLanguage.AddAlias("-l");
            bundleOptionNote.AddAlias("-n");
            bundleOptionOutput.AddAlias("-o");
            bundleOptionSort.AddAlias("-s");

            bundleOptionSort.SetDefaultValue(string.Empty);//היא ריקה sort option ברירת מחדל של  

            bundleCommand.AddOption(bundleOptionOutput);
            bundleCommand.AddOption(bundleOptionLanguage);
            bundleCommand.AddOption(bundleOptionNote);
            bundleCommand.AddOption(bundleOptionSort);
            bundleCommand.AddOption(bundleOptionDeleteEmptyLines);
            bundleCommand.AddOption(bundleOptionAuthor);

            bundleCommand.SetHandler((output, language, note, sort, remove, author) =>
            {
                try
                {
                    //...קבצים מסוגים bundle אין להכניס לקובץ ה 
                    string[] codeFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", System.IO.SearchOption.AllDirectories).
                    Where(file =>
                          !file.Contains("\\bin\\") &&
                          !file.Contains("\\obj\\") &&
                          !file.Contains("\\debug\\") &&
                          !file.Contains("\\release\\")).ToArray();
                    //language
                    if (language.Contains("all"))
                    {
                        language = codeFiles.Select(file => Path.GetExtension(file).TrimStart('.')).Distinct().ToArray();
                    }
                    //sort
                    IEnumerable<string> sortedFiles;
                    if (bundleOptionSort is not null && sort.Equals("type"))
                    {
                        sortedFiles = codeFiles.OrderBy(file => Path.GetExtension(file));
                    }
                    else
                    {
                        sortedFiles = codeFiles;
                    }

                    using (StreamWriter writer = new StreamWriter(output.FullName))
                    {
                        //author
                        if (author is not null)
                        {
                            writer.WriteLine("===============author===============");
                            writer.WriteLine($"name: {author}");
                        }
                        //bundle כתיבה לקןבץ ה
                        foreach (string codeFile in sortedFiles)
                        {
                            //רק את הקבצים משפה שהמשתמש בחר להציג
                            string fileExtension = Path.GetExtension(codeFile).TrimStart('.');
                            if (language.Contains(fileExtension))
                            {
                                isValid = true;
                                //note
                                if (note)
                                {
                                    writer.WriteLine("===============note===============");
                                    writer.WriteLine($"directory: {Directory.GetCurrentDirectory()}");
                                    writer.WriteLine($"name: {Path.GetFileName(codeFile)}");
                                }
                                //remove empty lines
                                string fileContent = File.ReadAllText(codeFile);
                                if (remove)
                                {
                                    fileContent = string.Join("\n", fileContent.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                                }
                                writer.WriteLine($"Code File:");
                                writer.WriteLine("=========================================");
                                writer.WriteLine(fileContent);
                                writer.WriteLine();
                            }

                        }
                        if (isValid == true)
                            Console.WriteLine("bundle file created succesfully");
                        else
                        {
                            Console.WriteLine("Note: the value you entered is invalid for language option. the bundle file is empty now.");
                        }
                    }
                }
                catch (DirectoryNotFoundException dr)
                {
                    Console.WriteLine(dr.Message);
                    Console.WriteLine("please enter a valid path.");
                    Console.WriteLine("If the file name consists of two words, write it in quotation marks ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }, bundleOptionOutput, bundleOptionLanguage, bundleOptionNote, bundleOptionSort, bundleOptionDeleteEmptyLines, bundleOptionAuthor);

            //response file command
            var rspCommand = new Command("create-rsp", "create response file with recommended options");

            rspCommand.AddOption(rspOptionOutput);
            rspCommand.SetHandler((output) =>
            {
                string[] options = new string[]
                {
                    "What is the recomemnded value for option 'output' ",
                    "What is the recomemnded value for option 'language' ",
                    "What is the recomemnded value for option 'note' ",
                    "What is the recomemnded value for option 'sort' ",
                    "What is the recomemnded value for option 'remove empty lines' ",
                    "What is the recomemnded value for option 'author' "
                };
                var answers = new string[options.Length];

                for (int i = 0; i < options.Length; i++)
                {
                    var question = options[i];
                    Console.WriteLine(question);
                    answers[i] = Console.ReadLine();
                }

                var rspContent = string.Join("\n", answers);
                File.WriteAllText(output, rspContent);

                Console.WriteLine("response file created succesfully");

            }, rspOptionOutput);
            var rootCommand = new RootCommand("root command for file bundler CLI");
            rootCommand.AddCommand(bundleCommand);
            rootCommand.AddCommand(rspCommand);
            rootCommand.InvokeAsync(args);
        }
    }

}
