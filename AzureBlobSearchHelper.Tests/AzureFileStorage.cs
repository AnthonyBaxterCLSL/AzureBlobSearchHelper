using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using NUnit.Framework;


namespace AzureBlobSearchHelper.Tests
{
    [TestFixture]
    public class AzureFileStorage
    {

        private class ValidTestObject
        {
            [MetaName]
            public string Dohickey { get; set; }

            public string Thingamabob { get; set; }
        }

        private class InvalidTestObject
        {
            public string NoMetaTag { get; set; }
        }

        [Test]
        public async Task WhenClassHasMetaName_GetsUsingName()
        {
            var t = new ValidTestObject() {Dohickey = "NameNameName", Thingamabob = "NotNameNotName"};

            
            var cont = new Moq.Mock<ICloudBlobContainer>();
            cont.Setup(container => container.GetBlockBlobReference(t.Dohickey))
                .Returns(new Mock<ICloudBlockBlob>().Object);

            var helper = new AzureFileStorage<ValidTestObject>(cont.Object, o => new byte[0]);
            await helper.TrySaveItemAsync(t);

            cont.Verify(container => container.GetBlockBlobReference(t.Dohickey), Times.Once);
        }

        [Test]
        public async Task WhenClassHasNoMetaName_ThrowsArgumentException()
        {
            var t = new InvalidTestObject();
            Assert.Throws<ArgumentException>(() =>
            {
                var helper = new AzureFileStorage<InvalidTestObject>(new Mock<ICloudBlobContainer>().Object,
                    o => new byte[0]);
            });

        } 
    }
}
