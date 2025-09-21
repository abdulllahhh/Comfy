using Model.Dtos.Request;
using Model.Dtos.Response;


namespace Model.Interface
{
    public interface IModelService
    {
        Task<WorkflowResponse> RunWorkflowAsync(string Id, WorkflowRequest requestt);
    }
}
