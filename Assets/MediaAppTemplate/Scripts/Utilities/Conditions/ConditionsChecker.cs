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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Daydream.MediaAppTemplate {

  /// This class is used to check conditions in the _ConditionManager_ by using a string expression
  /// that can be parsed at runtime. Each condition in the _ConditionManager_ can be referenced
  /// by using it's identifier.
  [System.Serializable]
  public class ConditionsChecker {
    public enum LogicSymbol {
      And,
      Or
    }

    [SerializeField]
    private string expression;
    private Expression<bool> parsedExpression;
  
    private static ExpressionParser<bool> expressionParser;

    public bool Evaluate() {
      if (string.IsNullOrEmpty(expression)) {
        return false;
      }
  
      if (expressionParser == null) {
        expressionParser = CreateExpressionParser();
      }
  
      if (parsedExpression == null) {
        parsedExpression = expressionParser.Parse(expression);
      }
  
      bool result = parsedExpression.Evaluate();
      return result;
    }

    private static ExpressionParser<bool> CreateExpressionParser() {
      ExpressionParser<bool> parser = new ExpressionParser<bool>();
  
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
  
      // Interpret literals via the ConditionsManager.
      parser.LiteralParser = (val) => ConditionsManager.Instance.IsConditionTrue(val);
  
      return parser;
    }
  }
}
