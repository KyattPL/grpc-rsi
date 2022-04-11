using Grpc.Core;
using GrpcServer;

namespace GrpcServer.Services
{
    public class ExerciseService : Exercise.ExerciseBase
    {
        private List<StudentDataModel> students;
        private readonly ILogger<ExerciseService> _logger;
        public ExerciseService(ILogger<ExerciseService> logger)
        {
            _logger = logger;
            students = new List<StudentDataModel>();
        }

        public override Task<Status> SendStudentData(StudentDataModel request, Grpc.Core.ServerCallContext context)
        {
            students.Add(request);
            return new Status { Message = "OK" };
        }

        public override Task<StudentDataModel> GetStudentData(GetStudentDataRequest request, Grpc.Core.ServerCallContext context)
        {
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
            await requestStream.ReadAsync();
            return new Status { Message = "OK" };
        }

        public override async Task DownloadImage(DownloadImageRequest request, Grpc.Core.IServerStreamWriter<ImageChunk> responseStream, Grpc.Core.ServerCallContext context)
        {   
            await responseStream.WriteAsync();
        }
    }
}