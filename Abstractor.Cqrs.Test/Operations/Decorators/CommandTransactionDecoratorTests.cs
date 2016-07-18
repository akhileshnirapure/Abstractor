using System;
using Abstractor.Cqrs.Infrastructure.Operations;
using Abstractor.Cqrs.Infrastructure.Operations.Decorators;
using Abstractor.Cqrs.Interfaces.CrossCuttingConcerns;
using Abstractor.Cqrs.Interfaces.Operations;
using Abstractor.Cqrs.Interfaces.Persistence;
using Abstractor.Cqrs.Test.Helpers;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using SharpTestsEx;
using Xunit;

namespace Abstractor.Cqrs.Test.Operations.Decorators
{
    public class CommandTransactionDecoratorTests
    {
        [Theory, AutoMoqData]
        public void Handle_WithoutLogAttribute_ShouldNotLog(
            [Frozen] Mock<ICommandHandler<ICommand>> commandHandler,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            [Frozen] Mock<ILogger> logger,
            [Frozen] Mock<IAttributeFinder> attributeFinder,
            ICommand command,
            CommandTransactionDecorator<ICommand> decorator)
        {
            // Arrange and assert

            attributeFinder.Setup(f => f.Decorates(command.GetType(), typeof (TransactionalAttribute))).Returns(true);

            var callOrder = 0;

            unitOfWork.Setup(d => d.Clear()).Callback(() => callOrder++.Should().Be(0));
            commandHandler.Setup(d => d.Handle(command)).Callback(() => callOrder++.Should().Be(1));
            unitOfWork.Setup(d => d.Commit()).Callback(() => callOrder++.Should().Be(2));

            // Act

            decorator.Handle(command);

            // Assert

            logger.Verify(l => l.Log(It.IsAny<string>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public void Handle_WithLogAttribute_ShouldLog(
            [Frozen] Mock<ICommandHandler<ICommand>> commandHandler,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            [Frozen] Mock<ILogger> logger,
            [Frozen] Mock<IAttributeFinder> attributeFinder,
            ICommand command,
            CommandTransactionDecorator<ICommand> decorator)
        {
            // Arrange and assert

            attributeFinder.Setup(f => f.Decorates(command.GetType(), typeof(TransactionalAttribute))).Returns(true);
            attributeFinder.Setup(f => f.Decorates(command.GetType(), typeof(LogAttribute))).Returns(true);

            // Act

            decorator.Handle(command);

            // Assert

            logger.Verify(l => l.Log(It.IsAny<string>()), Times.Exactly(2));
        }

        [Theory, AutoMoqData]
        public void Handle_WithoutTransactionalAttribute_ShouldNotCommit(
            [Frozen] Mock<ICommandHandler<ICommand>> commandHandler,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            [Frozen] Mock<ILogger> logger,
            [Frozen] Mock<IAttributeFinder> attributeFinder,
            ICommand command,
            CommandTransactionDecorator<ICommand> decorator)
        {
            // Act

            decorator.Handle(command);

            // Assert

            commandHandler.Verify(c => c.Handle(command), Times.Once);

            unitOfWork.Verify(u => u.Clear(), Times.Never);

            unitOfWork.Verify(u => u.Commit(), Times.Never);

            logger.Verify(l => l.Log(It.IsAny<string>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public void Handle_Success_ShouldCommitTheUnitOfWorkAfterCommandHandled(
            [Frozen] Mock<ICommandHandler<ICommand>> commandHandler,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            [Frozen] Mock<ILogger> logger,
            [Frozen] Mock<IAttributeFinder> attributeFinder,
            ICommand command,
            CommandTransactionDecorator<ICommand> decorator)
        {
            // Arrange and assert

            attributeFinder.Setup(f => f.Decorates(command.GetType(), typeof(TransactionalAttribute))).Returns(true);

            var callOrder = 0;

            unitOfWork.Setup(d => d.Clear()).Callback(() => callOrder++.Should().Be(0));
            commandHandler.Setup(d => d.Handle(command)).Callback(() => callOrder++.Should().Be(1));
            unitOfWork.Setup(d => d.Commit()).Callback(() => callOrder++.Should().Be(2));

            // Act

            decorator.Handle(command);
        }

        [Theory, AutoMoqData]
        public void Handle_Exception_ShouldNotCommitUnitOfWork(
            [Frozen] Mock<ICommandHandler<ICommand>> commandHandler,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            [Frozen] Mock<ILogger> logger,
            [Frozen] Mock<IAttributeFinder> attributeFinder,
            ICommand command,
            CommandTransactionDecorator<ICommand> decorator)
        {
            // Arrange

            attributeFinder.Setup(f => f.Decorates(command.GetType(), typeof(TransactionalAttribute))).Returns(true);

            commandHandler.Setup(d => d.Handle(It.IsAny<ICommand>())).Throws<Exception>();

            // Act

            Assert.Throws<Exception>(() => decorator.Handle(command));

            // Assert

            unitOfWork.Verify(d => d.Clear(), Times.Once);

            unitOfWork.Verify(d => d.Commit(), Times.Never);
        }
    }
}