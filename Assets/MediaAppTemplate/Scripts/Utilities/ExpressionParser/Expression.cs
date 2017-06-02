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
using System;
using System.Collections;
using System.Collections.Generic;

namespace Daydream.MediaAppTemplate {

  /// Represents an expression that evaluates to type T generated at runtime by an ExpressionParser.
  /// Hold a reference to this to evalute the same string expression multiple times without having to parse
  /// it multiple times.
  public class Expression<T> {
    private class Node {
      public bool IsLeaf;
      public Node Left;
      public Node Right;
      public string Value;
    }

    private string originalExpression;
    private Node rootNode;
    private ExpressionParser<T> parser;

    public Expression(List<Token> polishNotationTokens, ExpressionParser<T> expressionParser, string expression) {
      parser = expressionParser;
      originalExpression = expression;
      rootNode = CreateTree(polishNotationTokens);
    }

    public T Evaluate() {
      if (rootNode == null) {
        return default(T);
      }

      T result = EvaluateInternal(rootNode);
      return result;
    }

    private T EvaluateInternal(Node node) {
      if (node.IsLeaf) {
        Func<string, T> literalFunc = parser.LiteralParser;
        if (literalFunc != null) {
          return literalFunc(node.Value);
        }
      }

      // Unary
      if (node.Left != null && node.Right == null) {
        Func<T, T> unaryFunc = parser.GetUnaryFunc(node.Value);
        if (unaryFunc != null) {
          return unaryFunc(EvaluateInternal(node.Left));
        }
      }

      // Binary
      if (node.Left != null && node.Right != null) {
        Func<T, T, T> binaryFunc = parser.GetBinaryFunc(node.Value);
        if (binaryFunc != null) {
          T left = EvaluateInternal(node.Left);
          T right = EvaluateInternal(node.Right);
          return binaryFunc(left, right);
        }
      }

      Debug.LogError("Unsupported Operators in Expression. " +
        "OriginalExpression=\"" + originalExpression + "\" " +
        "Operator=" + node.Value);
      return default(T);
    }

    private static Node CreateTree(List<Token> polishNotationTokens) {
      List<Token>.Enumerator enumerator = polishNotationTokens.GetEnumerator();
      enumerator.MoveNext();
      Node node = MakeNode(ref enumerator);
      return node;
    }

    private static Node MakeNode(ref List<Token>.Enumerator polishNotationTokens) {
      Token token = polishNotationTokens.Current;

      if (token == null) {
        return null;
      }

      if (token.Type == SymbolType.Literal) {
        Node node = new Node();
        node.IsLeaf = true;
        node.Value = token.Value;
        polishNotationTokens.MoveNext();
        return node;
      } else if (token.Type == SymbolType.Operator) {
        if (token.isUnary) {
          Node node = new Node();
          node.Value = token.Value;
          polishNotationTokens.MoveNext();
          node.Left = MakeNode(ref polishNotationTokens);
          return node;
        } else {
          Node node = new Node();
          node.Value = token.Value;
          polishNotationTokens.MoveNext();
          node.Right = MakeNode(ref polishNotationTokens);
          node.Left = MakeNode(ref polishNotationTokens);
          return node;
        }
      } else {
        Debug.LogError("Invalid Token: " + token.Value + ", Type: " + token.Type);
      }
      return null;
    }
  }
}
