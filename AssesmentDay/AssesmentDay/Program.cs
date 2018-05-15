using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssesmentDay {
    class Program {
        static void Main(string[] args) {
            string salesFileLocation = string.Empty;
            string priceFileLocation = string.Empty;

            //get file locations from the arguments
            for (int i = 0; i < args.Length; i++) {
                if (args[i].ToLower() == "-s" && i + 1 < args.Length) {
                    salesFileLocation = args[i + 1];
                } else if (args[i].ToLower() == "-p" && i + 1 < args.Length) {
                    priceFileLocation = args[i + 1];
                }
            }

            try {
                //procced if file names have been entered and files exist
                if ((salesFileLocation != string.Empty && priceFileLocation != string.Empty) && (File.Exists(salesFileLocation) && File.Exists(priceFileLocation))) {
                    string reportOutput = string.Empty;

                    //Load and format data
                    Dictionary<string, float> coffeePrices = ReadPrice(priceFileLocation);
                    List<Sale> coffeeSales = ReadSale(salesFileLocation);

                    //generate report
                    reportOutput += "The most sold drink today was: " + CalculateMostPopularDrink(coffeeSales) + "\n";
                    reportOutput += "The most sold blend today was: " + CalculateMostPopularBlend(coffeeSales) + "\n";
                    reportOutput += "The most sold extra today was: " + CalculateMostPopularExtra(coffeeSales) + "\n";
                    reportOutput += "The percentage of extras was : " + CalculateExtraPercentage(coffeeSales) + "%\n";
                    reportOutput += "Todays income is: £" + CalculateTotalSales(coffeePrices, coffeeSales) + "\n";

                    //output information
                    Console.Write(reportOutput);

                    using (var writer = new StreamWriter("Report.txt")) {
                        writer.Write(reportOutput);
                    }
                } else {
                    throw new Exception("File arguments malformed");
                }
            } catch (Exception e) {
                Console.WriteLine("[Error] " + e.Message);
            }
        }

        private static float CalculateExtraPercentage(List<Sale> Sales) {
            float output = 0f;
            int noExtra = 0;

            foreach (var sale in Sales) {
                if(sale.Extras == null) {
                    noExtra++;
                }
            }

            //calc percentage
            if (noExtra != 0) {
                output = 100f - (float)(100f / Sales.Count) * noExtra;
            }

            return output;
        }

        private static float CalculateTotalSales(Dictionary<string, float> Prices, List<Sale> Sales) {
            float total = 0f;

            foreach (var sale in Sales) {
                total += Prices[sale.Drink];
            }

            return total;
        }

        #region code re-use corner weeeeee
        private static string CalculateMostPopularDrink(List<Sale> Sales) {
            //get most popular order
            Dictionary<string, int> saleCounter = new Dictionary<string, int>();

            foreach (var sale in Sales) {
                if (saleCounter.ContainsKey(sale.Drink)) {
                    saleCounter[sale.Drink]++;
                } else {
                    saleCounter.Add(sale.Drink, 1);
                }
            }

            //Find max value
            string output = string.Empty;
            int max = 0;

            foreach (var option in saleCounter) {
                if(option.Value > max) {
                    output = option.Key;
                    max = option.Value;
                } else if (option.Value == max) {
                    output += " & " + option.Key;
                }
            }

            return  output;
        }

        private static string CalculateMostPopularBlend(List<Sale> Sales) {
            //get most popular order
            Dictionary<string, int> saleCounter = new Dictionary<string, int>();

            foreach (var sale in Sales) {
                if (saleCounter.ContainsKey(sale.Roast)) {
                    saleCounter[sale.Roast]++;
                } else {
                    saleCounter.Add(sale.Roast, 1);
                }
            }

            //Find max value
            string output = string.Empty;
            int max = 0;

            foreach (var option in saleCounter) {
                if (option.Value > max) {
                    output = option.Key;
                    max = option.Value;
                } else if (option.Value == max) {
                    output += " & " + option.Key;
                }
            }

            return output;
        }

        private static string CalculateMostPopularExtra(List<Sale> Sales) {
            //get most popular order
            Dictionary<string, int> saleCounter = new Dictionary<string, int>();

            foreach (var sale in Sales) {
                if (sale.Extras != null && saleCounter.ContainsKey(sale.Extras)) {
                    saleCounter[sale.Extras]++;
                } else if (sale.Extras != null) {
                    saleCounter.Add(sale.Extras, 1);
                }
            }

            //Find max value
            string output = string.Empty;
            int max = 0;

            foreach (var option in saleCounter) {
                if (option.Value > max) {
                    output = option.Key;
                    max = option.Value;
                } else if (option.Value == max) {
                    output += " & " + option.Key;
                }
            }

            return output;
        }
        #endregion

        #region FileReaders
        private static Dictionary<string, float> ReadPrice(string fileName) {
            Dictionary<string, float> output = new Dictionary<string, float>();

            using (var reader = new StreamReader(fileName)) {
                //Grabheader and validate header
                var line = reader.ReadLine();

                if (line == "drink, price") {
                    while (!reader.EndOfStream) {
                        line = reader.ReadLine();
                        var values = line.Split(',');

                        //trim values and check everything has a value
                        for (int i = 0; i < values.Length; i++) {
                            values[i] = values[i].Trim();

                            if (values[i] == string.Empty) {
                                throw new Exception("Price data file error: Data malformed, has every value been filled in?");
                            }
                        }

                        output.Add(values[0], float.Parse(values[1]));
                    }
                } else {
                    throw new Exception("Price data file error: Header malformed, are you referencing the correct file?");
                }

                return output;
            }
        }

        private static List<Sale> ReadSale(string fileName) {
            List<Sale> output = new List<Sale>();

            using (var reader = new StreamReader(fileName)) {
                //Grabheader and validate header
                var line = reader.ReadLine();

                if (line == "drink, roast, quantity, extras") {
                    while (!reader.EndOfStream) {
                        line = reader.ReadLine();
                        var values = line.Split(',');

                        //trim values and check everything has a value
                        for (int i = 0; i < values.Length; i++) {
                            values[i] = values[i].Trim();

                            if(values[i] == string.Empty) {
                                throw new Exception("Sale data file error: Data malformed, has every value been filled in?");
                            }
                        }

                        //import the values into the sale list. Extras can have null values
                        output.Add(new Sale(values[0], values[1],int.Parse(values[2]),values[3]=="null"? null :values[3]));
                    }
                } else {
                    throw new Exception("Sale data file error: Header malformed, are you referencing the correct file?");
                }

                return output;
            }
        }
        #endregion

        public class Sale {
            public string Drink { get; private set; }
            public string Roast { get; private set; }
            public int Quantity { get; private set; }
            public string Extras { get; private set; }

            public Sale(string drink, string roast, int quantity, string extras) {
                Drink = drink;
                Roast = roast;
                Quantity = quantity;
                Extras = extras;
            }
        }
    }
}
