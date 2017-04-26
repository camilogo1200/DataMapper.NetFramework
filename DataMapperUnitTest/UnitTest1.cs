using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DataMapper;

namespace DataMapperUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        DataMapper<Usuario> mapper = DataMapper<Usuario>.Instancia;
#pragma warning disable CS0414 // The field 'UnitTest1.nEntities' is assigned but its value is never used
        private int nEntities = 2000;
#pragma warning restore CS0414 // The field 'UnitTest1.nEntities' is assigned but its value is never used
#pragma warning disable CS0414 // The field 'UnitTest1.initialEntitiesCount' is assigned but its value is never used
        private int initialEntitiesCount = 0;
#pragma warning restore CS0414 // The field 'UnitTest1.initialEntitiesCount' is assigned but its value is never used

        [TestInitialize]
        public void init() {

        }

        [TestMethod]
        public void shouldInstanceNotBeNull()
        {
            DataMapper<Usuario> mapper = DataMapper<Usuario>.Instancia;
            Assert.IsNotNull(mapper);
        }

        [TestMethod]
        public void shouldCreateNEntities()
        {
            Usuario u = new Usuario
            {
                password_hash = "123456",
                password_salt = "1213131",
                username = "TST"
            };
            for (int i = 0; i < 2000; i++)
            {
                mapper.Create(ref u);
            }


        }

        [TestMethod]
        public void shouldFindEntities()
        {
            String attribute = "username";
            String value = "Username";
            ICollection<Usuario> collection = mapper.findByAttribute(value, attribute, false);
        }
    }
}
