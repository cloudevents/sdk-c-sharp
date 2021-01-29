﻿// Copyright 2020 Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CloudNative.CloudEvents.UnitTests
{
    public class CloudEventAttributesTest
    {
        static readonly IEnumerable<ICloudEventExtension> emptyExtensions = new ICloudEventExtension[0];

        [Fact]
        public void Indexer_SetToNullValue_RegularAttribute()
        {
            var attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string attributeName = CloudEventAttributes.TypeAttributeName();
            attributes[attributeName] = "some event type";
            attributes[attributeName] = null;
            Assert.Null(attributes[attributeName]);
        }

        [Fact]
        public void Indexer_SetToNullValue_SpecVersion()
        {
            var attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string attributeName = CloudEventAttributes.SpecVersionAttributeName();
            Assert.Throws<InvalidOperationException>(() => attributes[attributeName] = null);
        }

        [Fact]
        public void Dictionary_Add_NullValue()
        {
            IDictionary<string, object> attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string attributeName = CloudEventAttributes.TypeAttributeName();
            Assert.Throws<ArgumentNullException>(() => attributes.Add(attributeName, null));
        }

        [Fact]
        public void Collection_Add_NullValue()
        {
            ICollection<KeyValuePair<string, object>> attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string attributeName = CloudEventAttributes.TypeAttributeName();
            var pair = KeyValuePair.Create(attributeName, default(object));
            Assert.Throws<InvalidOperationException>(() => attributes.Add(pair));
        }

        [Fact]
        public void Clear_PreservesSpecVersion()
        {
            IDictionary<string, object> attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string specVersionAttributeName = CloudEventAttributes.SpecVersionAttributeName();
            string specVersionValue = (string) attributes[specVersionAttributeName];
            attributes[CloudEventAttributes.TypeAttributeName()] = "some event type";
            Assert.Equal(2, attributes.Count);
            attributes.Clear();

            // We'd normally expect an empty dictionary now, but CloudEventAttributes always preserves the spec version.
            var entry = Assert.Single(attributes);
            Assert.Equal(CloudEventAttributes.SpecVersionAttributeName(), entry.Key);
            Assert.Equal(specVersionValue, entry.Value);
        }

        [Fact]
        public void Dictionary_Remove_SpecVersion()
        {
            IDictionary<string, object> attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string specVersionAttributeName = CloudEventAttributes.SpecVersionAttributeName();
            Assert.Throws<InvalidOperationException>(() => attributes.Remove(specVersionAttributeName));
        }

        [Fact]
        public void Collection_Remove_SpecVersion()
        {
            ICollection<KeyValuePair<string, object>> attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string specVersionAttributeName = CloudEventAttributes.SpecVersionAttributeName();
            // The value part is irrelevant; we throw on any attempt to remove a pair with a key that's the spec attribute version.
            var pair = KeyValuePair.Create(specVersionAttributeName, new object());
            Assert.Throws<InvalidOperationException>(() => attributes.Remove(pair));
        }

        [Theory]
        [InlineData("somekey", true)]
        [InlineData("some key", false)]
        [InlineData("Somekey", false)]
        [InlineData("somEkey", false)]
        [InlineData("1somekey3324", true)]
        public void Validates_LowerCase_Or_Digit(string key, bool isValid)
        {
            var attributes = new CloudEventAttributes(CloudEventsSpecVersion.Default, emptyExtensions);
            string value = "value";
            if (!isValid)
            {
                var exception = Assert.Throws<InvalidOperationException>(() => attributes[key] = value);
                Assert.StartsWith(string.Format(Strings.ErrorAttributeKeyIsNotWellFormed, key), exception.Message);

                exception = Assert.Throws<InvalidOperationException>(() => attributes.TryAdd(key, value));
                Assert.StartsWith(string.Format(Strings.ErrorAttributeKeyIsNotWellFormed, key), exception.Message);

                ICollection<KeyValuePair<string, object>> collection = attributes;
                exception = Assert.Throws<InvalidOperationException>(() => collection.Add(new KeyValuePair<string, object>(key, value)));
                Assert.StartsWith(string.Format(Strings.ErrorAttributeKeyIsNotWellFormed, key), exception.Message);
            }
            else
            {
                // should not throw
                attributes[key] = value;
                attributes.TryAdd(key, value);
                ICollection<KeyValuePair<string, object>> collection = attributes;
                collection.Clear();
                collection.Add(new KeyValuePair<string, object>(key, value));
            }
        }
    }
}
