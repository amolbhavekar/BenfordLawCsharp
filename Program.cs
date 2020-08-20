using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;


namespace BenfordModel
{
    class Program
    {
        static void Main(string[] args)
        {
            //double[] allTransactions = GetTransactionsFromFile();
            var storages = GetTransactionsFromFile();
            List<DigitFrequency> dfperVendor = new List<DigitFrequency>();

            List<double> fullAmountList = new List<double>();

            foreach (var storage in storages)
            {
                if (storage.Amounts.Count > 30)
                {
                    var df = new DigitFrequency();
                    double[] firstDigitFrequency = GetFirstDigitFrequency(storage.Amounts);
                    df.Vendor = storage.Vendor;
                    df.firstDigitFrequency = firstDigitFrequency;
                    dfperVendor.Add(df);
                }
                fullAmountList.AddRange(storage.Amounts);
            }

            //Full List Analysis
            double[] firstDigitFrequencyFull = GetFirstDigitFrequency(fullAmountList);
            OutputResults("All Vendors Cumulative", firstDigitFrequencyFull);



            //Individual vendor analysis with transactions more than 30 
            foreach (var df in dfperVendor)
            {
                //OutputResults(df.Vendor, df.firstDigitFrequency);
            }

            while (true)
            {
                Console.WriteLine("Please write vendor,Invoice_date,job_num,invoice_amt");

                var line = Console.ReadLine();

                string[] values = line.Split(',');

                if (dfperVendor.Any(x => x.Vendor == values[0]))
                {
                    var vendor = dfperVendor.Where(x => x.Vendor == values[0]).FirstOrDefault();
                    double myNum = 0;
                    if (Double.TryParse(values[3], out myNum) && myNum > 0)
                    {
                        OutputOneOrZero(vendor.Vendor, vendor.firstDigitFrequency, myNum);
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid amount");
                    }

                }
                else
                {
                    Console.WriteLine("Vendor not found");
                }
            }


        }

        private static void OutputResults(string vendor, double[] firstDigitFrequency)
        {

            double total = firstDigitFrequency.Sum();
            double frequencyPercentage;
            double benfordPercentage;

            Console.WriteLine("This analysis is for Vendor :" + vendor);
            for (int i = 1; i < 10; i++)
            {
                if (firstDigitFrequency[i] == 0)
                    break;
                frequencyPercentage = firstDigitFrequency[i] / total;
                benfordPercentage = Math.Log10(1 + 1.0 / i);
                Console.WriteLine("position: " + i + " frequency : " + frequencyPercentage + " Benford Percentage :" + benfordPercentage);
            }

            Console.WriteLine("Complete.");
        }


        private static void OutputOneOrZero(string vendor, double[] firstDigitFrequency, double vendorAmount)
        {
            double allowedTolerance = 0.5;
            double total = firstDigitFrequency.Sum();
            double frequencyPercentage;
            double benfordPercentage;
            int result = 0;

            int firstDigitOfNewAmount = int.Parse(vendorAmount.ToString().Substring(0, 1));

            for (int i = 1; i < 10; i++)
            {
                if (firstDigitFrequency[i] == 0)
                    break;
                frequencyPercentage = firstDigitFrequency[i] / total;
                benfordPercentage = Math.Log10(1 + 1.0 / i);
                if ((frequencyPercentage < (benfordPercentage * (1 - allowedTolerance))) || (frequencyPercentage > (benfordPercentage * (1 + allowedTolerance))))
                {
                    //Console.WriteLine("Review transactions with a first digit of: " + i);
                    if (firstDigitOfNewAmount == i)
                        //Console.WriteLine("0");
                        result = 0;
                    else
                        //Console.WriteLine("1");
                        result = 1;
                }
                else
                {
                    //Console.WriteLine("1");
                    result = 1;
                }

            }

            Console.WriteLine("Complete.");
        }

        private static double[] GetFirstDigitFrequency(List<double> allTransactions)
        {
            double[] firstDigitFrequency = new double[10];
            for (int i = 0; i < allTransactions.Count; i++)
            {
                int firstDigit = int.Parse(allTransactions[i].ToString().Substring(0, 1));
                firstDigitFrequency[firstDigit]++;
            }
            return firstDigitFrequency;
        }

        private static List<Storage> GetTransactionsFromFile()
        {
            try
            {

                var storages = new List<Storage>();
                int count = 0;

                //Reference System.Io.Compression.FileSystem
                string zipPath = @".\invoices.zip";
                string extractPath = @".\extract";


                //ZipFile.CreateFromDirectory(startPath, zipPath);
                if (File.Exists(@".\extract\invoices.csv"))
                {
                    File.Delete(@".\extract\invoices.csv");
                }

                ZipFile.ExtractToDirectory(zipPath, extractPath);

                foreach (var csvline in File.ReadLines(@".\extract\invoices.csv"))
                {
                    if (count != 0)
                    {
                        string[] values = csvline.Split(',');
                        double myNum = 0;
                        if (storages != null)
                        {
                            if (storages.Any(key => key.Vendor == values[0]))
                            {
                                if (Double.TryParse(values[3], out myNum) && myNum > 0)
                                {
                                    storages.Where(key => key.Vendor == values[0]).First().Amounts.Add(Convert.ToDouble(values[3]));
                                }

                            }
                            else
                            {

                                if (Double.TryParse(values[3], out myNum) && myNum > 0)
                                {
                                    var storage = new Storage();
                                    string Vendor = values[0];
                                    List<double> Amounts = new List<double>();
                                    Amounts.Add(Convert.ToDouble(values[3]));
                                    storage.Vendor = Vendor;
                                    storage.Amounts = Amounts;
                                    storages.Add(storage);
                                }

                            }
                        }
                    }
                    count++;
                }

                return storages;
            }
            catch (FileNotFoundException ex)
            {
                Console.Write(ex.Message);
                Console.WriteLine(" Did you copy the example file or create a new file at that location?");
            }
            return null;
        }
    }

    public class Storage
    {
        public string Vendor { get; set; }
        public List<double> Amounts { get; set; }
    }

    public class DigitFrequency
    {
        public string Vendor { get; set; }
        public double[] firstDigitFrequency { get; set; }
    }


}
