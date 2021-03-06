﻿using System.ComponentModel;
using Moq;
using ScaffoldR.Core.Commands;
using ScaffoldR.Core.Transactions;
using ScaffoldR.Infrastructure.Commands;
using ScaffoldR.Tests.Infrastructure.Commands.Fakes;
using Xunit;

namespace ScaffoldR.Tests.Infrastructure.Commands
{
    public class CommandTransactionDecoratorTests
    {
        [Fact]
        public void ExecuteTransaction_WhenCommandHasTransactionalAttribute()
        {
            var command = new FakeCommandWithoutValidator();
            Assert.IsAssignableFrom<ICommand>(command);
            
            var transactionProcessor = new Mock<IProcessTransactions>(MockBehavior.Strict);
            transactionProcessor.Setup(x => x.Execute());

            var decorated = new Mock<IHandleCommand<FakeCommandWithoutValidator>>(MockBehavior.Strict);
            var typeDescriptionProvider = TypeDescriptor.AddAttributes(decorated.Object.GetType(), new TransactionalAttribute());
            decorated.Setup(x => x.Handle(command));

            var decorator = new CommandTransactionDecorator<FakeCommandWithoutValidator>(transactionProcessor.Object, () => decorated.Object);
            decorator.Handle(command);

            decorated.Verify(x => x.Handle(command), Times.Once);
            transactionProcessor.Verify(x => x.Execute(), Times.Once);
            
            // Clean the provider for next test, or else it will fail - very odd.
            TypeDescriptor.RemoveProvider(typeDescriptionProvider, decorated.Object.GetType());
        }

        [Fact]
        public void DoNotExecuteTransaction_WhenCommandDoNotHaveTransactionalAttribute()
        {
            var command = new FakeCommandWithoutValidator();
            Assert.IsAssignableFrom<ICommand>(command);

            var transactionProcessor = new Mock<IProcessTransactions>(MockBehavior.Strict);
            transactionProcessor.Setup(x => x.Execute());

            var decorated = new Mock<IHandleCommand<FakeCommandWithoutValidator>>(MockBehavior.Strict);
            decorated.Setup(x => x.Handle(command));

            var decorator = new CommandTransactionDecorator<FakeCommandWithoutValidator>(transactionProcessor.Object, () => decorated.Object);
            decorator.Handle(command);

            decorated.Verify(x => x.Handle(command), Times.Once);
            transactionProcessor.Verify(x => x.Execute(), Times.Never);
        }

    }
}
