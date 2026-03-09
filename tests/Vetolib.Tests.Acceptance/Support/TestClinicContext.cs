using Vetolib.Shared.Kernel;

namespace Vetolib.Tests.Acceptance.Support;

internal class TestClinicContext : IClinicContext
{
    public Guid ClinicId { get; set; } = Guid.NewGuid();
}
