// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Daydream.MediaAppTemplate {
  [TestFixture]
  internal class ExpressionParserBooleanTest {

    private ExpressionParser<bool> parser;

    [SetUp]
    public void Setup() {
      parser = new ExpressionParser<bool>();

      // Or Symbol
      ExpressionParser<bool>.Symbol orSymbol = new ExpressionParser<bool>.Symbol();
      orSymbol.Name = "||";
      orSymbol.Type = SymbolType.Operator;
      orSymbol.RegexOverride = "\\|\\|";
      orSymbol.binaryFunc = (left, right) => left || right;
      parser.WithSymbol(orSymbol);

      // And Symbol
      ExpressionParser<bool>.Symbol andSymbol = new ExpressionParser<bool>.Symbol();
      andSymbol.Name = "&&";
      andSymbol.Type = SymbolType.Operator;
      andSymbol.binaryFunc = (left, right) => left && right;
      parser.WithSymbol(andSymbol);

      // Not Symbol
      ExpressionParser<bool>.Symbol notSymbol = new ExpressionParser<bool>.Symbol();
      notSymbol.Name = "!";
      notSymbol.Type = SymbolType.Operator;
      notSymbol.unaryFunc = (val) => !val;
      parser.WithSymbol(notSymbol);

      // Open Parenthesis
      ExpressionParser<bool>.Symbol openParenthesis = new ExpressionParser<bool>.Symbol();
      openParenthesis.Name = "(";
      openParenthesis.Type = SymbolType.OpenParenthesis;
      openParenthesis.RegexOverride = "\\(";
      parser.WithSymbol(openParenthesis);

      // Close Parenthesis
      ExpressionParser<bool>.Symbol closeParenthesis = new ExpressionParser<bool>.Symbol();
      closeParenthesis.Name = ")";
      closeParenthesis.Type = SymbolType.CloseParenthesis;
      closeParenthesis.RegexOverride = "\\)";
      parser.WithSymbol(closeParenthesis);

      // Interpret literals.
      parser.LiteralParser = (val) => val.ToLower() == "true";
    }

    [Test]
    public void Literals() {
      bool result = parser.Parse("true").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("false").Evaluate();
      Assert.IsFalse(result);
    }

    [Test]
    public void UnaryOperators() {
      bool result = parser.Parse("!true").Evaluate();
      Assert.IsFalse(result);

      result = parser.Parse("!!true").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("!false").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("!!false").Evaluate();
      Assert.IsFalse(result);
    }

    [Test]
    public void BinaryOperators() {
      bool result = parser.Parse("true || false").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("true || true").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("false || true").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("true && false").Evaluate();
      Assert.IsFalse(result);

      result = parser.Parse("true && true").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("false && true").Evaluate();
      Assert.IsFalse(result);
    }

    [Test]
    public void Parenthesis() {
      bool result = parser.Parse("true && (false || true)").Evaluate();
      Assert.IsTrue(result);

      result = parser.Parse("true && !(false || true)").Evaluate();
      Assert.IsFalse(result);

      result = parser.Parse("!(false || true)").Evaluate();
      Assert.IsFalse(result);
    }
  }
}
