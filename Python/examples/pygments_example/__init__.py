from pygments.formatter import Formatter
from pygments import highlight
from pygments.lexers.python import Python3Lexer
from pygments.filters import TokenMergeFilter
from pygments.token import *

# https://github.com/atelierbram/syntax-highlighting/blob/master/atelier-schemes/output/atom/base16-ateliercave-dark-base.less

# @import "base16-ateliercave-dark-syntax-variables.less";
# atom-text-editor, // <- remove when Shadow DOM can't be disabled
# :host {
#
# .variable.parameter.function {
#   color: #8b8792;
# }
#
# .comment, .punctuation.definition.comment {
#   color: #655f6d;
# }
#
# .punctuation.definition.string, .punctuation.definition.variable, .punctuation.definition.string, .punctuation.definition.parameters, .punctuation.definition.string, .punctuation.definition.array {
#   color: #8b8792;
# }
#
# .none {
#   color: #8b8792;
# }
#
# .keyword.operator {
#   color: #8b8792;
# }
#

# .string, .constant.other.symbol, .entity.other.inherited-class {
#   color: #2a9292;
# }
#
#
#
# .constant.other.color {
#   color: #398bc6;
# }
#
# .string.regexp {
#   color: #398bc6;
# }
#
# .constant.character.escape {
#   color: #398bc6;
# }
#
# .punctuation.section.embedded, .variable.interpolation {
#   color: #bf40bf;
# }
#
# .invalid.illegal {
#   color: #19171c;
#   background-color: #be4678;
# }

class TMPFormatter(Formatter):
    # source of actual color scheme
    def __init__(self):
        super().__init__()

        self.style_for =  {
            Comment:"#655f6d",
            Operator:"#8b8792",
            Keyword:"#955ae7",
            Other:"#8b8792",
            Generic:"#8b8792",
            Punctuation:"#8b8792",
            Literal:"#2a9292",
            Name:"#be4678",
            Error:"#be4678",
            Text:None, # whitespace
            Token:None # parent of all token types. Putting this here just in case something was missed
        }

    def format(self, tokensource, outfile):
        for ttype, value in tokensource:
            while ttype not in self.style_for:
                ttype = ttype.parent
            style = self.style_for[ttype]
            if style:
                outfile.write("<color={}>{}</color>".format(style, value))
            else:
                outfile.write(value)

def demo():
    source = "print('hello world')"
    lexer = Python3Lexer()
    lexer.add_filter(TokenMergeFilter())
    print(highlight(source,lexer, TMPFormatter()))

if __name__ == '__main__':
    demo()