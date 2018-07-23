from zmq_wrapper import ZmqBackendCmdLoop, init_backend_socket

from pygments.formatter import Formatter
from pygments import highlight
from pygments.lexers.python import Python3Lexer
from pygments.filters import TokenMergeFilter
from pygments.token import *

# source of actual color scheme
# https://github.com/atelierbram/syntax-highlighting/blob/master/atelier-schemes/output/atom/base16-ateliercave-dark-base.less


class TMPFormatter(Formatter):

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
            print("TokenType {} style is {}".format(ttype, style))
            if style:
                styled = "<color={}>{}</color>".format(style, value)
                outfile.write(styled)
            else:
                outfile.write(value)

class Highlighter(ZmqBackendCmdLoop):
    def __init__(self, host='localhost', port=5998, service_name='Syntax Highlighter'):
        socket = init_backend_socket(host, port, service_name)
        super().__init__(socket)

        self.lexer = Python3Lexer()
        self.lexer.add_filter(TokenMergeFilter())
        self.formatter = TMPFormatter()

    def do_highlight(self, source):
        return highlight(source, self.lexer, self.formatter)
