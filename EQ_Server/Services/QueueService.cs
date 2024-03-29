using AutoMapper;
using EQ_Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace EQ_Server.Services
{
    class QueueService : Queue.QueueBase
    {
        private readonly DataContext _db;
        private readonly IMapper mapper;

        public QueueService(DataContext db, IMapper mapper)
        {
            this._db = db;
            this.mapper=mapper;
        }
        [Authorize]
        public override Task<Empty> Enqueue(Empty request, ServerCallContext context)
        {
            var claim = context.GetHttpContext().User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (claim != null)
            {
                var userId = int.Parse(claim.Value);

                var queue = _db.Queues.FirstOrDefault(q => q.UserId == userId);
                if (queue != null)
                {
                    queue.Number = _db.Queues.Count() + 1;
                    _db.SaveChanges();
                }
            }
            return Task.FromResult(new Empty());
        }
        [Authorize]
        public override Task<Empty> Dequeue(Empty request, ServerCallContext context)
        {
            var claim = context.GetHttpContext().User.Claims.FirstOrDefault(c => c.Type == "Id");

            if (claim != null)
            {
                var userId = int.Parse(claim.Value);

                var queue = _db.Queues.FirstOrDefault(q => q.UserId == userId);

                if (queue != null)
                {
                    queue.Number = null;
                    _db.SaveChanges();
                }
            }
            return Task.FromResult(new Empty());
        }

        public override Task<Numbers> GetQueue(Empty request, ServerCallContext context)
        {
            var list = _db.Queues.Select(q => mapper.Map<EQ_Server.Models.Queue , Number>(q));
            var numbers = new Numbers();
            numbers.List.AddRange(list);
            return Task.FromResult(numbers);
        }
    }
}