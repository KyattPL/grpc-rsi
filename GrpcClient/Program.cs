using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System;

namespace GrpcClient
{
    class Program
    {
        static int FileSize = 155688;
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7041");
            var client = new Exercise.ExerciseClient(channel);
            var input = "";

            while (input != "q")
            {
                PrintMenu();
                Console.Write("Choose operation: ");
                input = Console.ReadLine()?.Trim();
                switch (input)
                {
                    case "1": AddStudent(client); break;
                    case "2": GetStudent(client); break;
                    case "3": UploadImage(client); break;
                    case "4": await DownloadImage(client); break;
                    default: break;
                }
            }

            Console.ReadLine();
        }

        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("1. Add student");
            Console.WriteLine("2. Get student");
            Console.WriteLine("3. Upload image");
            Console.WriteLine("4. Download image");
        }

        static void AddStudent(Exercise.ExerciseClient cl)
        {
            Console.Clear();
            Console.Write("Index number: ");
            int Index = Int32.Parse(Console.ReadLine().Trim());
            Console.Write("First name: ");
            string FirstName = Console.ReadLine().Trim();
            Console.Write("Last name: ");
            string LastName = Console.ReadLine().Trim();

            cl.SendStudentData(new StudentDataModel { Index=Index, FirstName=FirstName, LastName=LastName});
        }

        static async void GetStudent(Exercise.ExerciseClient cl)
        {
            Console.Clear();
            Console.Write("Index number: ");
            int Index = Int32.Parse(Console.ReadLine().Trim());

            var stud = await cl.GetStudentDataAsync(new GetStudentDataRequest { Index = Index, });
            Console.WriteLine($"\nIndex: {stud.Index}, First name: {stud.FirstName}, Last Name: {stud.LastName}");
        }

        static async void UploadImage(Exercise.ExerciseClient cl)
        {
            Console.Clear();
            Console.Write("File name: ");
            string FileName = Console.ReadLine().Trim();

            byte[] testImg = System.IO.File.ReadAllBytes($"./{FileName}");

            FileSize = testImg.Length;

            using (var call = cl.UploadImage())
            {
                await call.RequestStream.WriteAsync(new ImageChunk { Data = Google.Protobuf.ByteString.CopyFrom(testImg) });
            }

            await Task.Delay(500);
        }

        static async Task DownloadImage(Exercise.ExerciseClient cl)
        {
            var byteList = new List<byte>();
            var one_tenth = FileSize / 10;
            var counter = 0;
            var achievedStars = 0;

            PrintStars(0);

            using (var call = cl.DownloadImage(new DownloadImageRequest()))
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var currBytes = call.ResponseStream.Current;
                    if (currBytes.Data != null)
                    {
                        byteList.Add(currBytes.Data.ToList().FirstOrDefault());
                        counter++;

                        if (counter == one_tenth)
                        {
                            achievedStars++;
                            PrintStars(achievedStars);
                            counter = 0;
                        }
                    }
                }
            }
            
            await System.IO.File.WriteAllBytesAsync("./downloaded.png", byteList.ToArray());
            Console.WriteLine("DONE!");
            Console.ReadLine();
        }

        static void PrintStars(int NumOfStars)
        {
            Console.Clear();
            Console.WriteLine("Downloading:");
            Console.Write("[");
            for (int i = 1; i <= 10; i++)
            {
                if (i <= NumOfStars)
                {
                    Console.Write("*");
                } else
                {
                    Console.Write(" ");
                }
            }
            Console.WriteLine("]");
        }
    }
}