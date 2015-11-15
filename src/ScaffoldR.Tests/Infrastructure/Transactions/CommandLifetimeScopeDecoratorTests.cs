﻿using Moq;
using ScaffoldR.Core.Transactions;
using ScaffoldR.Infrastructure.Transactions;
using ScaffoldR.Tests.Infrastructure.CompositionRoot;
using ScaffoldR.Tests.Infrastructure.Transactions.Fakes;
using SimpleInjector;
using Xunit;

namespace ScaffoldR.Tests.Infrastructure.Transactions
{
    [Collection("Simple Injector Tests")]
    public class CommandLifetimeScopeDecoratorTests
    {
        private readonly CompositionRootFixture _fixture;

        public CommandLifetimeScopeDecoratorTests(CompositionRootFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void BeginsLifetimeScope_WhenCurrentLifetimeScope_IsNull()
        {
            var command = new FakeCommandWithValidator();
            var decorated = new Mock<IHandleCommand<FakeCommandWithValidator>>(MockBehavior.Strict);
            decorated.Setup(x => x.Handle(command));

            var decorator = new CommandLifetimeScopeDecorator<FakeCommandWithValidator>(_fixture.Container, () => decorated.Object);
            Assert.Equal(null, _fixture.Container.GetCurrentLifetimeScope());
            decorator.Handle(command);

            Assert.Equal(null, _fixture.Container.GetCurrentLifetimeScope());
            decorated.Verify(x => x.Handle(command), Times.Once);
        }

        [Fact]
        public void UsesCurrentLifetimeScope_WhenCurrentLifetimeScope_IsNotNull()
        {
            var command = new FakeCommandWithValidator();
            var decorated = new Mock<IHandleCommand<FakeCommandWithValidator>>(MockBehavior.Strict);
            decorated.Setup(x => x.Handle(command));

            var decorator = new CommandLifetimeScopeDecorator<FakeCommandWithValidator>(_fixture.Container, () => decorated.Object);
            Assert.Equal(null, _fixture.Container.GetCurrentLifetimeScope());

            using (_fixture.Container.BeginLifetimeScope())
            {
                decorator.Handle(command);
            }

            Assert.Equal(null, _fixture.Container.GetCurrentLifetimeScope());

            decorated.Verify(x => x.Handle(command), Times.Once);
        }
    }
}
