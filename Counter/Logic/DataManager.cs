using System;
using System.Linq;
using System.Threading.Tasks;
using Counter.Model;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Counter.Logic
{
    public class DataManager
    {
        private static StorageFolder localFolder;
        private static ObservableCollection<CounterItem> tempCollection;
        private static string fileName = "data.json";

        public DataManager()
        {
            localFolder = ApplicationData.Current.LocalFolder;
        }

        public Task<ObservableCollection<CounterItem>> GetDataAsync = new Task<ObservableCollection<CounterItem>>(() =>
        {
            ObservableCollection<CounterItem> _tempCollection;

            /// Get data from local json file
            try
            {
                _tempCollection = LoadData();
            } catch(Exception ex)
            {
                Debug.WriteLine(String.Format(
                    "There was a problem while loading the data.\nError message: {0}",
                    ex.Message));
                _tempCollection = null;
            }
            return _tempCollection;
        });

        private static ObservableCollection<CounterItem> LoadData()
        {
            try
            {
                LoadFromJSONFile();
                return tempCollection;
            } catch(Exception)
            { return new ObservableCollection<CounterItem>(); }
        }

        private static async void LoadFromJSONFile()
        {
            try
            {
                tempCollection = new ObservableCollection<CounterItem>();
                StorageFile jsonFile = await localFolder.GetFileAsync(fileName);
                using (IRandomAccessStream textStream = await jsonFile.OpenReadAsync())
                {
                    using (DataReader textReader = new DataReader(textStream))
                    {
                        uint length = (uint)textStream.Size;
                        await textReader.LoadAsync(length);
                        string jsonContents = textReader.ReadString(length);
                        if (jsonContents.Trim() != "")
                        {
                            var tempList = ((JArray)JsonConvert.DeserializeObject(jsonContents)).ToList();
                            foreach (var item in tempList)
                            {
                                try
                                {
                                    if (item != null)
                                    {
                                        tempCollection.Add(new CounterItem(tempCollection)
                                        {
                                            ID = Int32.Parse(item["ID"].ToString()),
                                            Title = item["Title"].ToString(),
                                            Count = Int32.Parse(item["Count"].ToString()),
                                            Description = item["Description"].ToString(),
                                            CreationDate = DateTime.Parse(item["CreationDate"].ToString()),
                                            ModificationDate = DateTime.Parse(item["ModificationDate"].ToString())

                                        });
                                    }
                                } catch (Exception ex) { Debug.WriteLine("Error while loading data: " + ex.Message); }
                            }
                        }
                    }
                }
            } catch(System.IO.FileNotFoundException)
            {
                Debug.WriteLine("The file does not exist");
                CreateFile(fileName);
            } catch(Exception ex)
            {
                Debug.WriteLine("Unknown error.\nMessage: " + ex.Message);
            }
        }

        private static async void CreateFile(string name)
        {
            await localFolder.CreateFileAsync(name);
        }

        public async void SaveData(ObservableCollection<CounterItem> _collection)
        {
            try
            {
                string jsonContents = JsonConvert.SerializeObject(_collection, Formatting.Indented);
                StorageFile jsonFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (IRandomAccessStream textStream = await jsonFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (DataWriter textWriter = new DataWriter(textStream))
                    {
                        textWriter.WriteString(jsonContents);
                        await textWriter.StoreAsync();
                    }
                }
            } catch (Exception ex)
            {
                // Do a backup of the data
            }
        }

        private static ObservableCollection<CounterItem> GetTestData()
        {
            ObservableCollection<CounterItem> _tempCollection = new ObservableCollection<CounterItem>();
            _tempCollection.Add(new CounterItem(_tempCollection) { Title = "Item 1", Count = 8 });
            _tempCollection.Add(new CounterItem(_tempCollection) { Title = "Item 2", Count = 10 });
            _tempCollection.Add(new CounterItem(_tempCollection) { Title = "Item 3", Count = 5 });
            _tempCollection.Add(new CounterItem(_tempCollection) { Title = "Item 4", Count = 1 });
            _tempCollection.Add(new CounterItem(_tempCollection) { Title = "Item 5", Count = 7 });
            return _tempCollection;
        }
    }
}
