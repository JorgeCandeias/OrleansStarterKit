﻿using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Silo.Tests
{
    public class RemoveVersionFromParameterOperationFilterTests
    {
        [Fact]
        public void Applies_Filter()
        {
            // arrange
            var filter = new RemoveVersionFromParameterOperationFilter();
            var parameter = new Mock<IParameter>();
            parameter.Setup(_ => _.Name).Returns("version");
            var operation = new Operation
            {
                Parameters = new List<IParameter>
                {
                    parameter.Object
                }
            };
            var context = new OperationFilterContext(new ApiDescription(), Mock.Of<ISchemaRegistry>(), Mock.Of<MethodInfo>());

            // act
            filter.Apply(operation, context);

            // assert
            Assert.Empty(operation.Parameters);
        }

        [Fact]
        public void Skips_Filter()
        {
            // arrange
            var filter = new RemoveVersionFromParameterOperationFilter();
            var parameter = new Mock<IParameter>();
            parameter.Setup(_ => _.Name).Returns("other");
            var operation = new Operation
            {
                Parameters = new List<IParameter>
                {
                    parameter.Object
                }
            };
            var context = new OperationFilterContext(new ApiDescription(), Mock.Of<ISchemaRegistry>(), Mock.Of<MethodInfo>());

            // act
            filter.Apply(operation, context);

            // assert
            Assert.Single(operation.Parameters, parameter.Object);
        }
    }
}
