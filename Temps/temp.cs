[HttpPut("updateProjectCertificate/{workEffortId}")]
public async Task<ActionResult<ProjectCertificateRecord>> UpdateProjectCertificate(
    string workEffortId,
    [FromBody] UpdateProjectCertificateCommand command)
{
    command.Certificate.WorkEffortId = workEffortId;
    var result = await Mediator.Send(new UpdateProjectCertificate.Command { Certificate = command.Certificate });
    return HandleResult(result);
}