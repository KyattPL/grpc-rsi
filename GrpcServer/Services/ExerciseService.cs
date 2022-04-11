using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services
{
    public class ExerciseService : Exercise.ExerciseBase
    {
        private static List<StudentDataModel> students = new List<StudentDataModel>();
        private readonly ILogger<ExerciseService> _logger;
        public ExerciseService(ILogger<ExerciseService> logger)
        {
            _logger = logger;
        }

        public override Task<Status> SendStudentData(StudentDataModel request, Grpc.Core.ServerCallContext context)
        {
            students.Add(request);
            return Task.FromResult(new Status { Message = "OK" });
        }

        public override async Task<StudentDataModel> GetStudentData(GetStudentDataRequest request, Grpc.Core.ServerCallContext context)
        {
            await Task.Delay(1000);
            int indexToFind =  request.Index;
            foreach (var stud in students) {
                if (stud.Index == indexToFind) {
                    return stud;
                }
            }
            return null;
        }

        public override async Task<Status> UploadImage(Grpc.Core.IAsyncStreamReader<ImageChunk> requestStream, Grpc.Core.ServerCallContext context)
        {
            await requestStream.MoveNext();
            var byteArr = requestStream.Current.Data.ToArray();
            await System.IO.File.WriteAllBytesAsync("./uploaded.png", byteArr);
            return await Task.FromResult(new Status { Message = "OK" });
        }

        public override async Task DownloadImage(DownloadImageRequest request, Grpc.Core.IServerStreamWriter<ImageChunk> responseStream, Grpc.Core.ServerCallContext context)
        {
            var byteArr = await System.IO.File.ReadAllBytesAsync("./uploaded.png");
            foreach (var singleByte in byteArr)
            {
                await responseStream.WriteAsync(new ImageChunk { Data = Google.Protobuf.ByteString.CopyFrom(singleByte) });
            }
        }
    }
}