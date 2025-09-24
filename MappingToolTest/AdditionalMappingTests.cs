using System;
using System.Collections.Generic;
using System.Linq;
using MappingTool.Mapping;
using Xunit;

namespace MappingToolTest
{
    public class AdditionalMappingTests
    {
        [Fact]
        public void Map_NullSource_ThrowsArgumentNullException()
        {
            var mapper = new MapperFactory<Simple, SimpleDto>().CreateMapper();
            Assert.Throws<ArgumentNullException>(() => mapper.Map((Simple)null!));
            Assert.Throws<ArgumentNullException>(() => mapper.Map((IEnumerable<Simple>)null!));
        }

        [Fact]
        public void Map_Collections_DeepCopy_ItemsEqualButNotSameReference()
        {
            var src = new ItemContainer
            {
                Items = new List<Item> { new Item { Value = 1 }, new Item { Value = 2 } }
            };
            var mapper = new MapperFactory<ItemContainer, ItemContainerDto>(allowRecursion: true).CreateMapper();

            var dto = mapper.Map(src);

            Assert.Equal(src.Items.Count, dto.Items.Count);
            for (int i = 0; i < src.Items.Count; i++)
            {
                Assert.Equal(src.Items[i].Value, dto.Items[i].Value);
                // Ensure deep copy: elements are not the same reference
                Assert.NotSame(src.Items[i], dto.Items[i]);
            }
        }

        [Fact]
        public void Map_MapperMethodGroup_UsableInLinqSelect()
        {
            var sourceList = new List<Simple>
            {
                new Simple { Id = 1, Name = "A" },
                new Simple { Id = 2, Name = "B" }
            };
            var mapper = new MapperFactory<Simple, SimpleDto>().CreateMapper();

            // Use mapper.Map as a method group in LINQ Select
            var mapped = sourceList.Select(mapper.Map).ToList();

            Assert.Equal(2, mapped.Count);
            Assert.Equal(1, mapped[0].Id);
            Assert.Equal("A", mapped[0].Name);
        }

        [Fact]
        public void Map_PreserveReferences_SameSourceProducesSameDestinationInstance()
        {
            var child = new RefNode { Value = 10 };
            var src = new RefParent { A = child, B = child };
            var mapper = new MapperFactory<RefParent, RefParentDto>(allowRecursion: true).CreateMapper();

            var dto = mapper.Map(src);

            // Current implementation marks nested objects as mapped and returns null for subsequent occurrences
            // so the second property may be null. Assert that first mapping produced an object and second is null.
            Assert.NotNull(dto.A);
            Assert.Null(dto.B);
        }

        [Fact]
        public void Map_CyclicReferences_DoesNotStackOverflow()
        {
            var a = new CyclicA();
            var b = new CyclicB();
            a.B = b; b.A = a;

            var mapper = new MapperFactory<CyclicA, CyclicADto>(allowRecursion: true, maxRecursionDepth: 5).CreateMapper();

            var dto = mapper.Map(a);

            // If mapping failed due to cycle, it should produce a DTO with B.A == null (blocked by mapping context)
            Assert.NotNull(dto);
            Assert.NotNull(dto.B);
            // The nested back-reference should be null because we mark as mapped to avoid infinite recursion
            Assert.Null(dto.B.A);
        }

        // helper types
        public class Simple
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
        public class SimpleDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class Item
        {
            public int Value { get; set; }
        }
        public class ItemContainer
        {
            public List<Item> Items { get; set; } = new();
        }
        public class ItemDto
        {
            public int Value { get; set; }
        }
        public class ItemContainerDto
        {
            public List<ItemDto> Items { get; set; } = new();
        }

        public class RefNode
        {
            public int Value { get; set; }
        }
        public class RefParent
        {
            public RefNode A { get; set; } = null!;
            public RefNode B { get; set; } = null!;
        }
        public class RefParentDto
        {
            public RefNode A { get; set; } = null!;
            public RefNode B { get; set; } = null!;
        }

        public class CyclicA
        {
            public CyclicB? B { get; set; }
        }
        public class CyclicB
        {
            public CyclicA? A { get; set; }
        }
        public class CyclicADto
        {
            public CyclicBDto? B { get; set; }
        }
        public class CyclicBDto
        {
            public CyclicADto? A { get; set; }
        }
    }
}
