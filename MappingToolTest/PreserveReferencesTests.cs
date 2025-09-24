using System;
using System.Collections.Generic;
using System.Linq;
using MappingTool.Mapping;
using Xunit;

namespace MappingToolTest
{
    public class PreserveReferencesTests
    {
        [Fact]
        public void PreserveReferences_SameSourceReference_MapsToSameDestinationInstance()
        {
            var child = new Node { Value = 123 };
            var src = new Parent { A = child, B = child };

            var mapper = new MapperFactory<Parent, ParentDto>(allowRecursion: true, preserveReferences: true).CreateMapper();

            var dto = mapper.Map(src);

            Assert.NotNull(dto);
            Assert.NotNull(dto.A);
            Assert.NotNull(dto.B);
            Assert.Same(dto.A, dto.B);
        }

        [Fact]
        public void PreserveReferences_DuplicateInCollection_PreservesReferences()
        {
            var child = new Node { Value = 5 };
            var srcList = new List<Node> { child, child };

            var mapper = new MapperFactory<Node, Node>(preserveReferences: true).CreateMapper();

            var mapped = mapper.Map(srcList).ToList();

            Assert.Equal(2, mapped.Count);
            Assert.Same(mapped[0], mapped[1]);
        }

        public class Node
        {
            public int Value { get; set; }
        }

        public class Parent
        {
            public Node? A { get; set; }
            public Node? B { get; set; }
        }

        public class NodeDto
        {
            public int Value { get; set; }
        }

        public class ParentDto
        {
            public NodeDto? A { get; set; }
            public NodeDto? B { get; set; }
        }
    }
}
