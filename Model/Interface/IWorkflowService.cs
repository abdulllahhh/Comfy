using Model.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interface
{
    public interface IWorkflowService
    {
        Task<object> RunModelAsync(string userId, WorkflowRequest request);
    }
}
