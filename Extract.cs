using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MC_Sound_Texture_Extractor
{
    internal class Extract
    {
        private List<SoundsData> soundsData;

        public Extract()
        {
            soundsData = new List<SoundsData>();
        }
        public void Run()
        {
            //get sound directory GLOBAL VARIABLE
            Console.WriteLine("Hello, I will move you through the process of extracting all your sound files.\n" +
            "First we need the Hashed folder location, this should be located in %AppData%\\.minecraft\\assets\\objects\n" +
            "Please put the full directory path here: ");
            string soundDirectory = "";
            while (true)
            {
                soundDirectory = Console.ReadLine();
                if (Directory.Exists(soundDirectory))
                {
                    break;
                }
                else if (soundDirectory == null || soundDirectory == "")
                {
                    Console.WriteLine("You did not enter a directory, please try again.");
                }
                else
                {
                    Console.WriteLine("That directory does not exist, please try again.");
                }
            }
            Globals.SoundDirectory = soundDirectory;

            //get <version>.json location GLOBAL VARIABLE
            Console.WriteLine("Now we need the location of the <version>.json file, this should be located in %AppData%\\.minecraft\\assets\\indexes\n" +
                "Please put the full file path here: ");
            string versionJsonLocation = "";
            while (true)
            {
                versionJsonLocation = Console.ReadLine();
                if (File.Exists(versionJsonLocation))
                {
                    break;
                }
                else if (versionJsonLocation == null || versionJsonLocation == "")
                {
                    Console.WriteLine("You did not enter a file path, please try again.");
                }
                else
                {
                    Console.WriteLine("That file does not exist, make sure the path includes the file and try again.");
                }
            }
            Globals.VersionJsonLocation = versionJsonLocation;

            //get output folder GLOBAL VARIABLE
            Console.WriteLine("Finally, we need the location of the output folder, this is where all the sound files will be extracted to.\n" +
                "Please put the full directory path here: ");
            string outputFolder = "";
            while (true)
            {
                outputFolder = Console.ReadLine();
                if (Directory.Exists(outputFolder))
                {
                    break;
                }
                else if (outputFolder == null || outputFolder == "")
                {
                    Console.WriteLine("You did not enter a directory, please try again.");
                }
                else
                {
                    Console.WriteLine("That directory does not exist, please try again.");
                }
            }
            Globals.OutputFolder = outputFolder;

            Console.WriteLine("Press any key to begin extraction");
            Console.ReadKey();
            Console.Clear();

            //read <version>.json and make a list of all the sound files
            MakeList();

            //make corresponding direcories using <version>.json
            MakeDirectories();

            //copy over all the sound files in a temp folder
            copyFiles();

            //move all the sound files to their corresponding directories
            moveFiles();

            //delete temp folder and it's contents
            deleteTemp();
        }

        public void MakeList()
        {
            //open and read <version>.json
            Console.WriteLine("Opening " + Globals.VersionJsonLocation);

            //read the json file
            string json;
            using (StreamReader sr = new StreamReader(Globals.VersionJsonLocation))
            {
                json = sr.ReadToEnd();
            }

            Console.WriteLine($"JSON Loaded: {json.Length} characters");

            //parse the json file CHATGPT WAS USED TO HELP WITH THIS
            var results = new Dictionary<string, string>(); // To store file path and hash
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("objects", out JsonElement objectsElement))
                {
                    // Loop through each property under "objects"
                    foreach (JsonProperty property in objectsElement.EnumerateObject())
                    {
                        string key = property.Name;
                        if (key.StartsWith("minecraft/sounds", StringComparison.OrdinalIgnoreCase))
                        {
                            // Use the full file path (key) and extract the hash
                            string filePathKey = key; // Use the entire file path
                            string hash = property.Value.GetProperty("hash").GetString();
                            results[filePathKey] = hash;
                        }
                    
                    }
                }
                else
                {
                    Console.WriteLine("The 'objects' property was not found in the JSON.");
                }
            }


            //Add to the list of sound files
            foreach (var entry in results)
            {
                Console.WriteLine($"File Path: {entry.Key}, Hash: {entry.Value}");
                soundsData.Add(new SoundsData(entry.Value, entry.Key));
            }

            //get the name of the sound file and the folder path
            foreach (SoundsData sound in soundsData)
            {
                sound.GetName();
            }
        }

        public void MakeDirectories()
        {
            //make directories using the folder path
            Console.WriteLine("Making directories");

            soundsData.ForEach(sound =>
            {
                string folderPath = sound.folderPath;
                string newFolderPath = Path.Combine(Globals.OutputFolder, folderPath);
                if (!Directory.Exists(newFolderPath)) {
                    Console.WriteLine(newFolderPath);
                    Directory.CreateDirectory(newFolderPath);
                }
                else
                {
                    Console.WriteLine("Directory already exists: " + newFolderPath);
                }
            });
        }
        public void copyFiles()
        {
            //make temp folder
            Console.WriteLine("Making temp folder");
            string tempFolder = Path.Combine(Globals.OutputFolder, "temp");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            else
            {
                Console.WriteLine("Temp folder already exists: " + tempFolder);
            }

            //copy all the sound files to the temp folder
            Console.WriteLine("Copying files to temp folder");

            string[] hashFiles = Directory.GetFiles(Globals.SoundDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string file in hashFiles)
            {
                try
                {
                    string filePath = Path.Combine(tempFolder, Path.GetFileName(file));
                    Console.WriteLine("Copying " + file + " To " + filePath);
                    File.Copy(file, filePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error copying file: " + file + ". " + e.Message);
                }
            }
            Console.WriteLine("Successfully copied all files to temp folder");
        }
        public void moveFiles()
        {
            //move and rename all the sound files to their corresponding names in <version>.json
            Console.WriteLine("Moving and renaming temp files");

            foreach (SoundsData sound in soundsData)
            {
                string oldFilePath = Path.Combine(Globals.OutputFolder, "temp", sound.hash);
                string newFilePath = Path.Combine(Globals.OutputFolder, sound.folderPath, sound.name);
                try
                {
                    Console.WriteLine("Moving " + oldFilePath + " To " + newFilePath);
                    File.Move(oldFilePath, newFilePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error moving file: " + oldFilePath + ". " + e.Message);
                }
            }
            Console.WriteLine("Successfully moved all files");
        }
        public void deleteTemp()
        {
            //delete temp folder and all files included
            Console.WriteLine("Deleting temp folder");
            string tempFolder = Path.Combine(Globals.OutputFolder, "temp");
            try
            {
                Directory.Delete(tempFolder, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error deleting temp folder: " + tempFolder + ". " + e.Message);
            }
            Console.WriteLine("Successfully deleted temp folder");
        }
    }
}
