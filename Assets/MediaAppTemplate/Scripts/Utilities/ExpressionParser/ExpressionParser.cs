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
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace Daydream.MediaAppTemplate {

  public enum SymbolType {
    OpenParenthesis,
    CloseParenthesis,
    Operator,
    Literal
  }

  public class Token {
    public SymbolType Type;
    public string Value;
    public bool isUnary;
  }

  /// This class is used to parse a string expression and evaluate it as type T.
  /// An ExpressionParser must be provided with Symbols to be used. Symbols can be Unary, Binary, or both.
  /// The ExpressionParser also must be provided with a function to interpret string literals as type T.
  /// Anything that isn't a Symbol is considered a literal.
  /// See _ExpressionParserFloatTest_ and ExpressionParserBooleanTest for examples.
  public class ExpressionParser<T> {
    public class Symbol {
      public string Name;
      public SymbolType Type;
      public int Precedence;
      public string RegexOverride;
      public Func<T, T> unaryFunc;
      public Func<T, T, T> binaryFunc;
    }

    private Dictionary<string, Symbol> symbolDictionary = new Dictionary<string, Symbol>();
    private string regexPattern;

    public Func<string, T> LiteralParser { get; set; }

    public void WithSymbol(Symbol op) {
      symbolDictionary[op.Name] = op;
      regexPattern = null;
    }

    public Func<T, T> GetUnaryFunc(string symbolName) {
      Symbol symbol;
      if (symbolDictionary.TryGetValue(symbolName, out symbol)) {
        return symbol.unaryFunc;
      }

      return null;
    }

    public Func<T, T, T> GetBinaryFunc(string symbolName) {
      Symbol symbol;
      if (symbolDictionary.TryGetValue(symbolName, out symbol)) {
        return symbol.binaryFunc;
      }

      return null;
    }

    public Expression<T> Parse(string expression) {
      List<Token> inFixTokens = Tokenize(expression);
      List<Token> polishNotationTokens = ConvertToPolishNotation(inFixTokens);
      return new Expression<T>(polishNotationTokens, this, expression);
    }

    private List<Token> Tokenize(string expression) {
      List<Token> result = new List<Token>();

      if (string.IsNullOrEmpty(regexPattern)) {
        foreach (KeyValuePair<string, Symbol> pair in symbolDictionary) {
          string symbol = !string.IsNullOrEmpty(pair.Value.RegexOverride) ?
            pair.Value.RegexOverride : pair.Value.Name;

          if (string.IsNullOrEmpty(regexPattern)) {
            regexPattern = symbol;
          } else {
            regexPattern += "|" + symbol;
          }
        }
      }

      Regex regex = new Regex(regexPattern, RegexOptions.IgnorePatternWhitespace);
      MatchCollection matches = regex.Matches(expression);
      int finalIndex = 0;

      for (int i = 0; i < matches.Count; i++) {
        Match match = matches[i];

        // If we are the first match, check if there is a literal before the first operator
        if (i == 0) {
          if (match.Index != 0) {
            AddLiteralValue(expression, 0, match.Index, result);
          }
        }

        Token operatorToken = new Token();
        operatorToken.Type = symbolDictionary[match.Value].Type;
        operatorToken.Value = match.Value;
        result.Add(operatorToken);

        // If we aren't the last match, check if there is a literal before the next operator
        if (i < matches.Count - 1) {
          Match nextMatch = matches[i + 1];
          int startIndex = match.Index + match.Length;
          int endIndex = nextMatch.Index;
          int length = endIndex - startIndex;
          if (length > 0) {
            AddLiteralValue(expression, startIndex, length, result);
          }
        } else {
          finalIndex = match.Index + match.Length;
        }
      }

      if (finalIndex < expression.Length) {
        int length = expression.Length - finalIndex;
        AddLiteralValue(expression, finalIndex, length, result);
      }

      return result;
    }

    private void AddLiteralValue(string expression, int startIndex, int length, List<Token> tokens) {
      string literalValue = expression.Substring(startIndex, length);
      literalValue = literalValue.Trim();
      if (!string.IsNullOrEmpty(literalValue)) {
        Token literalToken = new Token();
        literalToken.Type = SymbolType.Literal;
        literalToken.Value = literalValue;
        tokens.Add(literalToken);
      }
    }

    private List<Token> ConvertToPolishNotation(List<Token> inFixTokens) {
      Stack<Token> operators = new Stack<Token>();
      Queue<Token> outputQueue = new Queue<Token>();

      for (int i = 0; i < inFixTokens.Count; i++) {
        Token token = inFixTokens[i];
        switch (token.Type) {
          case SymbolType.Literal:
            outputQueue.Enqueue(token);
            break;
          case SymbolType.Operator:
            bool isUnary = isOperatorUnary(i, inFixTokens);
            if (isUnary) {
              token.isUnary = true;
            } else {
              while (operators.Count > 0
                && operators.Peek().Type != SymbolType.OpenParenthesis
                && IsHigerPrecedence(token, operators.Peek())) {
                outputQueue.Enqueue(operators.Pop());
              }
            }
            operators.Push(token);
            break;
          case SymbolType.OpenParenthesis:
            operators.Push(token);
            break;
          case SymbolType.CloseParenthesis:
            while (operators.Count > 0 && operators.Peek().Type != SymbolType.OpenParenthesis) {
              outputQueue.Enqueue(operators.Pop());
            }
            operators.Pop();
            if (operators.Count > 0 && operators.Peek().isUnary) {
              outputQueue.Enqueue(operators.Pop());
            }
            break;
          default:
            break;
        }
      }

      while (operators.Count > 0) {
        outputQueue.Enqueue(operators.Pop());
      }

      return outputQueue.Reverse().ToList();
    }

    private bool isOperatorUnary(int index, List<Token> inFixTokens) {
      Token token = inFixTokens[index];

      if (token.Type != SymbolType.Operator) {
        return false;
      }

      if (index == 0) {
        return true;
      }

      Token previousToken = inFixTokens[index - 1];
      if (previousToken.Type == SymbolType.Operator) {
        return true;
      }

      return false;
    }

    private bool IsHigerPrecedence(Token a, Token b) {
      return symbolDictionary[b.Value].Precedence >= symbolDictionary[a.Value].Precedence;
    }
  }
}
