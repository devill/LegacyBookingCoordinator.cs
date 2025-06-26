using VerifyXunit;
using Xunit;

namespace LegacyTestingTools.Tests
{
    public class ObjectFactoryTests
    {
        [Fact]
        public void ObjectFactory_Instance_ShouldReturnSameInstance()
        {
            var instance1 = ObjectFactory.Instance();
            var instance2 = ObjectFactory.Instance();

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void ObjectFactory_Create_WithConcreteType_ShouldCreateInstance()
        {
            var factory = new ObjectFactory();

            var result = factory.Create<TestClass>("param1");

            Assert.NotNull(result);
            Assert.IsType<TestClass>(result);
            Assert.Equal("param1", result.Value);
        }

        [Fact]
        public void ObjectFactory_Create_WithInterfaceAndImplementation_ShouldCreateInstance()
        {
            var factory = new ObjectFactory();

            var result = factory.Create<ITestInterface, TestImplementation>("param1");

            Assert.NotNull(result);
            Assert.IsAssignableFrom<ITestInterface>(result);
            Assert.IsType<TestImplementation>(result);
            Assert.Equal("param1", result.Value);
        }

        [Fact]
        public void ObjectFactory_SetOne_ShouldUseProvidedInstanceOnce()
        {
            var factory = new ObjectFactory();
            var customInstance = new TestClass("custom");

            factory.SetOne<TestClass>(customInstance);
            var result1 = factory.Create<TestClass>("ignored");
            var result2 = factory.Create<TestClass>("normal");

            Assert.Same(customInstance, result1);
            Assert.NotSame(customInstance, result2);
            Assert.Equal("custom", result1.Value);
        }

        [Fact]
        public void ObjectFactory_SetAlways_ShouldAlwaysUseProvidedInstance()
        {
            var factory = new ObjectFactory();
            var customInstance = new TestClass("always");

            factory.SetAlways<TestClass>(customInstance);
            var result1 = factory.Create<TestClass>("ignored1");
            var result2 = factory.Create<TestClass>("ignored2");

            Assert.Same(customInstance, result1);
            Assert.Same(customInstance, result2);
        }

        [Fact]
        public void ObjectFactory_SetOne_WithInterface_ShouldUseProvidedInstanceOnce()
        {
            var factory = new ObjectFactory();
            var customInstance = new TestImplementation("interface-custom");

            factory.SetOne<ITestInterface>(customInstance);
            var result1 = factory.Create<ITestInterface, TestImplementation>("ignored");
            var result2 = factory.Create<ITestInterface, TestImplementation>("normal");

            Assert.Same(customInstance, result1);
            Assert.NotSame(customInstance, result2);
        }

        [Fact]
        public void ObjectFactory_SetAlways_WithInterface_ShouldAlwaysUseProvidedInstance()
        {
            var factory = new ObjectFactory();
            var customInstance = new TestImplementation("interface-always");

            factory.SetAlways<ITestInterface>(customInstance);
            var result1 = factory.Create<ITestInterface, TestImplementation>("ignored1");
            var result2 = factory.Create<ITestInterface, TestImplementation>("ignored2");

            Assert.Same(customInstance, result1);
            Assert.Same(customInstance, result2);
        }

        [Fact]
        public void ObjectFactory_Clear_ShouldRemoveTypeOverrides()
        {
            var factory = new ObjectFactory();
            var customInstance = new TestClass("custom");

            factory.SetAlways<TestClass>(customInstance);
            factory.Clear<TestClass>();
            var result = factory.Create<TestClass>("normal");

            Assert.NotSame(customInstance, result);
            Assert.Equal("normal", result.Value);
        }

        [Fact]
        public void ObjectFactory_ClearAll_ShouldRemoveAllOverrides()
        {
            var factory = new ObjectFactory();
            var customClass = new TestClass("custom");
            var customInterface = new TestImplementation("custom");

            factory.SetAlways<TestClass>(customClass);
            factory.SetAlways<ITestInterface>(customInterface);
            factory.ClearAll();

            var classResult = factory.Create<TestClass>("normal");
            var interfaceResult = factory.Create<ITestInterface, TestImplementation>("normal");

            Assert.NotSame(customClass, classResult);
            Assert.NotSame(customInterface, interfaceResult);
        }

        [Fact]
        public void ObjectFactory_WithConstructorCalledWith_ShouldNotifyTarget()
        {
            var factory = new ObjectFactory();
            var testInstance = new TestClassWithConstructorCallback("test", 123);

            factory.SetOne<IConstructorCalledWith>(testInstance);
            var result = factory.Create<IConstructorCalledWith, TestClassWithConstructorCallback>("test", 123);

            Assert.NotNull(result);
            Assert.Same(testInstance, result);
            Assert.True(testInstance.ConstructorWasCalled);
            Assert.Equal(2, testInstance.ArgumentCount);
        }

        [Fact]
        public void ObjectFactory_Create_WithNoArguments_ShouldCreateInstance()
        {
            var factory = new ObjectFactory();

            var result = factory.Create<SimpleTestClass>();

            Assert.NotNull(result);
            Assert.IsType<SimpleTestClass>(result);
        }

        [Fact]
        public void ObjectFactory_SetOne_MultipleTypes_ShouldTrackSeparately()
        {
            var factory = new ObjectFactory();
            var customClass = new TestClass("custom1");
            var customInterface = new TestImplementation("custom2");

            factory.SetOne<TestClass>(customClass);
            factory.SetOne<ITestInterface>(customInterface);

            var classResult1 = factory.Create<TestClass>("ignored");
            var interfaceResult1 = factory.Create<ITestInterface, TestImplementation>("ignored");
            var classResult2 = factory.Create<TestClass>("normal");
            var interfaceResult2 = factory.Create<ITestInterface, TestImplementation>("normal");

            Assert.Same(customClass, classResult1);
            Assert.Same(customInterface, interfaceResult1);
            Assert.NotSame(customClass, classResult2);
            Assert.NotSame(customInterface, interfaceResult2);
        }

        [Fact]
        public void ObjectCreation_StaticMethods_ShouldDelegateToSingleton()
        {
            try
            {
                var customInstance = new TestClass("singleton-test");
                ObjectFactory.Instance().SetOne<TestClass>(customInstance);

                var result = ObjectCreation.Create<TestClass>("ignored");

                Assert.Same(customInstance, result);
            }
            finally
            {
                ObjectFactory.Instance().ClearAll();
            }
        }

        [Fact]
        public void ObjectCreation_StaticMethods_WithInterface_ShouldDelegateToSingleton()
        {
            try
            {
                var customInstance = new TestImplementation("singleton-interface-test");
                ObjectFactory.Instance().SetOne<ITestInterface>(customInstance);

                var result = ObjectCreation.Create<ITestInterface, TestImplementation>("ignored");

                Assert.Same(customInstance, result);
            }
            finally
            {
                ObjectFactory.Instance().ClearAll();
            }
        }
    }

    public class TestClass
    {
        public string Value { get; }

        public TestClass(string value)
        {
            Value = value;
        }
    }

    public class SimpleTestClass
    {
        public SimpleTestClass() { }
    }

    public interface ITestInterface
    {
        string Value { get; }
    }

    public class TestImplementation : ITestInterface
    {
        public string Value { get; }

        public TestImplementation(string value)
        {
            Value = value;
        }
    }

    public class TestClassWithConstructorCallback : IConstructorCalledWith
    {
        public bool ConstructorWasCalled { get; private set; }
        public int ArgumentCount { get; private set; }

        public TestClassWithConstructorCallback(string param1, int param2) { }

        public void ConstructorCalledWith(params object[] args)
        {
            ConstructorWasCalled = true;
            ArgumentCount = args?.Length ?? 0;
        }
    }
}