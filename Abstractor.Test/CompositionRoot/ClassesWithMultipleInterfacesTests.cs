using System.Collections.Generic;
using Abstractor.Cqrs.Interfaces.Events;
using Abstractor.Cqrs.Interfaces.Operations;
using Abstractor.Test.Helpers;
using SharpTestsEx;
using Xunit;

namespace Abstractor.Test.CompositionRoot
{
    public class ClassesWithMultipleInterfacesTests : BaseTest
    {
        // Classes with multiple interfaces should be registered correctly
        public class FakeRepository : IFake1Repository, IFake2Repository
        {
        }

        public interface IFake1Repository
        {
        }

        public interface IFake2Repository
        {
        }

        public class FakeCommand : ICommand
        {
            public IFake1Repository Repository1 { get; set; }

            public IFake2Repository Repository2 { get; set; }
        }

        public class FakeCommandHandler : ICommandHandler<FakeCommand>
        {
            private readonly IFake1Repository _fake1Repository;
            private readonly IFake2Repository _fake2Repository;

            public FakeCommandHandler(IFake1Repository fake1Repository, IFake2Repository fake2Repository)
            {
                _fake1Repository = fake1Repository;
                _fake2Repository = fake2Repository;
            }

            public IEnumerable<IDomainEvent> Handle(FakeCommand command)
            {
                command.Repository1 = _fake1Repository;
                command.Repository2 = _fake2Repository;
                 
                yield break;
            }
        }

        [Fact]
        public void ClassWithTwoInterfaces_BothShouldBeInjectedCorrectly()
        {
            // Arrange

            var command = new FakeCommand();

            // Act

            CommandDispatcher.Dispatch(command);

            // Assert

            (command.Repository1 is FakeRepository).Should().Be.True();

            (command.Repository2 is FakeRepository).Should().Be.True();
        }
    }
}