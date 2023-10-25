#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace mro {
  public sealed class html {
    // Called at the start of an html tag. We look forward and record information
    // about our tag. Handles start tags, close tags, and solo tags. 'Collects'
    // an entire tag.
    // <returns>Tag name.</returns>
    static private string LookAhead(string html,
                                     int start,
                                     out bool isClose,
                                     out bool isSolo) {
      isClose = false;
      isSolo = false;

      StringBuilder tagName = new StringBuilder();

      // Stores the position of the final slash
      int slashPos = -1;
      // Whether we have encountered a space
      bool space = false;
      // Whether we are in a quote
      bool quote = false;
      // out limit to look foor
      int ncars = html.Length;

      // Begin scanning the tag
      for (int i = 0; ; ++i) {
        // Get the position in main html
        int pos = start + i;
        // Don't go outside the html
        if (pos >= ncars) return "x";
        // The character we are looking at
        char c = html[pos];
        // See if a space has been encountered
        if (char.IsWhiteSpace(c)) space = true;
        // Add to our tag name if none of these are present
        if (space == false && c != '<' && c != '>' && c != '/') tagName.Append(c);
        // Record position of slash if not inside a quoted area
        if (c == '/' && quote == false) slashPos = i;
        // End at the > bracket
        if (c == '>') break;
        // Record whether we are in a quoted area
        if (c == '\"') quote = !quote;
      }

      // Determine if this is a solo or closing tag
      if (slashPos != -1) {
        // If slash is at the end so this is solo
        if (html[slashPos + 1] == '>') isSolo = true;
        else isClose = true;
      }

      // Return the name of the tag collected
      if (tagName.Length == 0) return "empty";
      else return tagName.ToString();
    }

    // Tags that must be closed in the start
    static Dictionary<string, bool> _soloTags = new Dictionary<string, bool>() {
			//{"img", true},
			{"br", true}
      };

    // Whether the HTML is likely valid. Error parameter will be empty
    // if no errors were found.
    static public void CheckHtml(string html, out string error) {
      // Store our tags in a stack
      Stack<string> tags = new Stack<string>();

      // Initialize out parameter to empty
      error = string.Empty;
      int row = 1;
      int col = 1;

      // Count of parenthesis
      int parenthesisR = 0;
      int parenthesisL = 0;

      // Traverse entire HTML
      int ncars = html.Length;
      for (int i = 0; i < ncars; ++i, ++col) {
        char c = html[i];
        if (c == '<') {
          bool isClose;
          bool isSolo;

          // Look ahead at this tag
          string tag = LookAhead(html, i, out isClose, out isSolo);

          // Make sure tag is lowercase
          if (tag.ToLower() != tag) {
            error = "upper: " + tag + ", row: " + row.tostr() +
                ", col: " + col.tostr() + ", pos: " + i.tostr();
            return;
          }

          // Make sure solo tags are parsed as solo tags
          if (_soloTags.ContainsKey(tag)) {
            if (!isSolo) {
              error = "!solo: " + tag + ", row: " + row.tostr() +
                  ", col: " + col.tostr() + ", pos: " + i.tostr();
              return;
            }
          }
          else {
            // We are on a regular end or start tag
            if (isClose) {
              // We can't close a tag that isn't on the stack
              if (tags.Count == 0) {
                error = "closing: " + tag + ", row: " + row.tostr() +
                    ", col: " + col.tostr() + ", pos: " + i.tostr();
                return;
              }

              // Tag on stack must be equal to this closing tag
              if (tags.Peek() == tag) {
                // Remove the start tag from the stack
                tags.Pop();
              }
              else {
                // Mismatched closing tag
                error = "!match: " + tag + ", row: " + row.tostr() +
                    ", col: " + col.tostr() + ", pos: " + i.tostr();
                return;
              }
            }
            else {
              // Add tag to stack
              tags.Push(tag);
            }
          }
          i += tag.Length;
        }
        else if (c == '&') {
          // & must never be followed by space or other &
          if ((i + 1) < html.Length) {
            char next = html[i + 1];

            if (char.IsWhiteSpace(next) || next == '&') {
              error = "error: ampersand, row: " + row.tostr() +
                  ", col: " + col.tostr() + ", pos: " + i.tostr();
              return;
            }
          }
        }
        else if (c == '\t') {
          error = "err: tab, row: " + row.tostr() + ", col: " +
              col.tostr() + ", pos: " + i.tostr();
          return;
        }
        else if (c == '(') {
          parenthesisL++;
        }
        else if (c == ')') {
          parenthesisR++;
        }
        else if (c == '\n') {
          ++row;
          col = 1;
        }
      }

      // If we have tags in the stack, write them to error
      foreach (string tagName in tags) {
        error += "extra:" + tagName + " ";
      }

      // Require even number of parenthesis
      if (parenthesisL != parenthesisR) {
        error = "!even ";
      }
    }
  }
}
