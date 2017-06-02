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
  internal class ExpressionParserFloatTest {

    private ExpressionParser<float> parser;

    [SetUp]
    public void Setup() {
      parser = new ExpressionParser<float>();

      // Plus Symbol
      ExpressionParser<float>.Symbol plusSymbol = new ExpressionParser<float>.Symbol();
      plusSymbol.Name = "+";
      plusSymbol.Type = SymbolType.Operator;
      plusSymbol.Precedence = 1;
      plusSymbol.RegexOverride = "\\+";
      plusSymbol.binaryFunc = (left, right) => left + right;
      parser.WithSymbol(plusSymbol);

      // Minus Symbol
      ExpressionParser<float>.Symbol minusSymbol = new ExpressionParser<float>.Symbol();
      minusSymbol.Name = "-";
      minusSymbol.Type = SymbolType.Operator;
      minusSymbol.Precedence = 2;
      minusSymbol.RegexOverride = "\\-";
      minusSymbol.unaryFunc = (val) => -val;
      minusSymbol.binaryFunc = (left, right) => left - right;
      parser.WithSymbol(minusSymbol);

      // Multiply Symbol
      ExpressionParser<float>.Symbol multSymbol = new ExpressionParser<float>.Symbol();
      multSymbol.Name = "*";
      multSymbol.Type = SymbolType.Operator;
      multSymbol.Precedence = 3;
      multSymbol.RegexOverride = "\\*";
      multSymbol.binaryFunc = (left, right) => left * right;
      parser.WithSymbol(multSymbol);

      // Divide Symbol
      ExpressionParser<float>.Symbol divideSymbol = new ExpressionParser<float>.Symbol();
      divideSymbol.Name = "/";
      divideSymbol.Type = SymbolType.Operator;
      divideSymbol.Precedence = 4;
      divideSymbol.RegexOverride = "\\/";
      divideSymbol.binaryFunc = (left, right) => left / right;
      parser.WithSymbol(divideSymbol);

      // Open Parenthesis
      ExpressionParser<float>.Symbol openParenthesis = new ExpressionParser<float>.Symbol();
      openParenthesis.Name = "(";
      openParenthesis.Type = SymbolType.OpenParenthesis;
      openParenthesis.RegexOverride = "\\(";
      parser.WithSymbol(openParenthesis);

      // Close Parenthesis
      ExpressionParser<float>.Symbol closeParenthesis = new ExpressionParser<float>.Symbol();
      closeParenthesis.Name = ")";
      closeParenthesis.Type = SymbolType.CloseParenthesis;
      closeParenthesis.RegexOverride = "\\)";
      parser.WithSymbol(closeParenthesis);

      // Interpret literals.
      parser.LiteralParser = (val) => float.Parse(val);
    }

    [Test]
    public void Literals() {
      float result = parser.Parse("1.6").Evaluate();
      Assert.AreEqual(result, 1.6f);

      result = parser.Parse("0").Evaluate();
      Assert.AreEqual(result, 0);
    }

    [Test]
    public void UnaryOperators() {
      float result = parser.Parse("-1").Evaluate();
      Assert.AreEqual(result, -1.0f);

      result = parser.Parse("--2.5").Evaluate();
      Assert.AreEqual(result, 2.5f);
    }

    [Test]
    public void BinaryOperators() {
      float result = parser.Parse("2 + 2").Evaluate();
      Assert.AreEqual(result, 4.0f);

      result = parser.Parse("2 - 2").Evaluate();
      Assert.AreEqual(result, 0);

      result = parser.Parse("4 * 4").Evaluate();
      Assert.AreEqual(result, 16.0f);

      result = parser.Parse("8 / 2").Evaluate();
      Assert.AreEqual(result, 4.0f);

      result = parser.Parse("2 + 3 * 2").Evaluate();
      Assert.AreEqual(result, 8.0f);

      result = parser.Parse("2 - 5 * 4 / 2").Evaluate();
      Assert.AreEqual(result, -8.0f);
    }

    [Test]
    public void Parenthesis() {
      float result = parser.Parse("(2 + 3) * 2").Evaluate();
      Assert.AreEqual(result, 10.0f);

      result = parser.Parse("-(2 + 3) * 2").Evaluate();
      Assert.AreEqual(result, -10.0f);

      result = parser.Parse("2 - (5 * (4 / 4)) - 2").Evaluate();
      Assert.AreEqual(result, -5.0f);
    }
  }
}
