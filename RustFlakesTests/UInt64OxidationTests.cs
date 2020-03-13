﻿// Copyright (c) 2013 - Jeremiah Peschka
// Copyright (c) 2013 - Mike Haboustak
//
// This file is provided to you under the Apache License,
// Version 2.0 (the "License"); you may not use this file
// except in compliance with the License.  You may obtain
// a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using NUnit.Framework;
using RustFlakes;
using System.Threading;

namespace RustFlakesTests
{
    [TestFixture]
    public class UInt64OxidationTests
    {
        // 16-bits = 4 hex digits
        internal ushort WorkerId;

        [SetUp]
        public void Setup()
        {
            WorkerId = 0xabcd;
        }

        [Test]
        public void ShouldMaintain16BitWorkerId()
        {
            var oxidation = new UInt64Oxidation(WorkerId);
            var key = oxidation.Oxidize();
            var id = (ushort) (key >> 16 & 0xffff);

            Assert.AreEqual(WorkerId, id);
        }

        [Test]
        public void SequentialKeysAreSequential()
        {
            var oxidation = new UInt64Oxidation(WorkerId);

            var key = oxidation.Oxidize();
            var key2 = oxidation.Oxidize();
            var key3 = oxidation.Oxidize();

            System.Threading.Thread.Sleep(10);
            var key4 = oxidation.Oxidize();
            var key5 = oxidation.Oxidize();

            Assert.IsTrue(key5 > key4 && key4 > key3 && key3 > key2 && key2 > key);
        }
        [Test]
        public void InDefaultOxidationShouldNotMaintain32BitTimestamp()
        {
            var oxidation = new UInt64Oxidation(WorkerId);

            var key1 = oxidation.Oxidize();
            Thread.Sleep(10);
            var key2 = oxidation.Oxidize();
            Thread.Sleep(10);
            var key3 = oxidation.Oxidize();
            Thread.Sleep(10);

            var timestamp1 = (uint)(key1 >> 32);
            var timestamp2 = (uint)(key2 >> 32);
            var timestamp3 = (uint)(key3 >> 32);

            Assert.IsTrue((timestamp1 != timestamp2) && (timestamp2 != timestamp3));
        }

        [Test]
        public void InSlowOxidationShouldMaintain32BitTimestamp()
        {
            ushort intervalInMs = 1000; // 1 second
            var oxidation = new UInt64Oxidation(WorkerId, intervalInMs);
            
            var key1 = oxidation.Oxidize();
            Thread.Sleep(10);
            var key2 = oxidation.Oxidize();
            Thread.Sleep(10); 
            var key3 = oxidation.Oxidize();
            Thread.Sleep(10);

            var timestamp1 = (uint)(key1 >> 32);
            var timestamp2 = (uint)(key2 >> 32);
            var timestamp3 = (uint)(key3 >> 32);

            Assert.IsTrue((timestamp1 == timestamp2) || (timestamp2 == timestamp3));
        }
    }
}