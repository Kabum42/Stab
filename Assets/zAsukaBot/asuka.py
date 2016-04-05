# -*- coding: utf-8 -*-
import tweepy
import glob
import random
import os
from random import randint

#Personal, every user should complete.
api_key = "Qd4HxsrVkW6iN8rsSAsVPGonq"
api_secret = "xWG6GdoQ2bB5C52DuY3s9Ol3FUXom8kdIaty9F7UoUoBZeCi9y"
oauth_token = "4056791182-RRJYWxTlmgPZWnY7UAsIvQAZkPJe4TowJqRirjv" # Access Token
oauth_token_secret = "P5ptCdg7mMnLh3s120y3PlasLGLGQJi1nUkMJQjD68APm" # Access Token Secret
auth = tweepy.OAuthHandler(api_key, api_secret)
auth.set_access_token(oauth_token, oauth_token_secret)
api = tweepy.API(auth)

# EARS
ear1 = [];
ear2 = [];

def addEars(a, b):
	ear1.append(a);
	ear2.append(b);

addEars(u"q", u"p");
addEars(u"ʢ", u"ʡ");
addEars(u"⸮", u"?");
addEars(u"ʕ", u"ʔ");
addEars(u"ᖗ", u"ᖘ");
addEars(u"ᕦ", u"ᕥ");
addEars(u"ᕦ(", u")ᕥ");
addEars(u"ᕙ(", u")ᕗ");
addEars(u"ᘳ", u"ᘰ");
addEars(u"ᕮ", u"ᕭ");
addEars(u"ᕳ", u"ᕲ");
addEars(u"(", u")");
addEars(u"[", u"]");
addEars(u"¯\_", u"_/¯");
addEars(u"୨", u"୧");
addEars(u"⤜(", u")⤏");
addEars(u"☞", u"☞");
addEars(u"ᑫ", u"ᑷ");
addEars(u"ヽ(", u")ﾉ");
addEars(u"\(", u")/");
addEars(u"乁(", u")ㄏ");
addEars(u"└[", u"]┘");
addEars(u"(ง", u")ง");
addEars(u"|", u"|");
addEars(u"(づ", u")づ");

# EYES
eye1 = [];
eye2 = [];

def addEyes(a, b):
	eye1.append(a);
	eye2.append(b);

addEyes(u"⌐■", u"■");
addEyes(u" ͠°", u" °");
addEyes(u"⇀", u"↼");
addEyes(u"´• ", u" •`");
addEyes(u"´", u"`");
addEyes(u"`", u"´");
addEyes(u"ó", u"ò");
addEyes(u"ò", u"ó");
addEyes(u"⸌", u"⸍");
addEyes(u">", u"<");
addEyes(u"ᗒ", u"ᗕ");
addEyes(u"⪧", u"⪦");
addEyes(u"⪦", u"⪧");
addEyes(u"⪩", u"⪨");
addEyes(u"⪨", u"⪩");
addEyes(u"⪰", u"⪯");
addEyes(u"⫑", u"⫒");
addEyes(u"⨴", u"⨵");
addEyes(u"⩿", u"⪀");
addEyes(u"⩾", u"⩽");
addEyes(u"⩺", u"⩹");
addEyes(u"⩹", u"⩺");
addEyes(u"◥▶", u"◀◤");
addEyes(u"≋", u"≋");
addEyes(u"૦ઁ", u"૦ઁ");
addEyes(u"  ͯ", u"  ͯ");
addEyes(u"  ̿", u"  ̿");
addEyes(u"  ͌", u"  ͌");
addEyes(u"܍", u"܍");
addEyes(u"◉", u"◉");
addEyes(u"☉", u"☉");
addEyes(u"・", u"・");
addEyes(u"▰", u"▰");
addEyes(u"ᵔ", u"ᵔ");
addEyes(u" ﾟ", u" ﾟ");
addEyes(u"◕", u"◕");
addEyes(u"◔", u"◔");
addEyes(u"✧", u"✧");
addEyes(u"■", u"■");
addEyes(u"☼", u"☼");
addEyes(u"*", u"*");
addEyes(u"⚆", u"⚆");
addEyes(u"⊜", u"⊜");
addEyes(u"❍", u"❍");
addEyes(u"￣", u"￣");
addEyes(u"─", u"─");
addEyes(u"✿", u"✿");
addEyes(u"•", u"•");
addEyes(u"T", u"T");
addEyes(u"^", u"^");
addEyes(u"ⱺ", u"ⱺ");
addEyes(u"@", u"@");
addEyes(u"ȍ", u"ȍ");
addEyes(u"x", u"x");
addEyes(u"-", u"-");
addEyes(u"๏", u"๏");
addEyes(u"ⴲ", u"ⴲ");
addEyes(u"♥", u"♥");
addEyes(u"¬", u"¬");
addEyes(u" º ", u" º ");
addEyes(u"⨶", u"⨶");
addEyes(u"⨱", u"⨱");
addEyes(u"⍜", u"⍜");
addEyes(u"⍤", u"⍤");
addEyes(u"ᚖ", u"ᚖ");
addEyes(u"ᴗ", u"ᴗ");
addEyes(u"ಠ", u"ಠ");
addEyes(u"σ", u"σ");

# MOUTH

mouth = [];

mouth.append(u"v");
mouth.append(u"ᴥ");
mouth.append(u"ᗝ");
mouth.append(u"ᗜ");
mouth.append(u"ᨓ");
mouth.append(u"╭͜ʖ╮");
mouth.append(u" ͜ʖ");
mouth.append(u" ͟ʖ");
mouth.append(u" ʖ̯");
mouth.append(u"ω");
mouth.append(u" ³");
mouth.append(u"﹏");
mouth.append(u"‿");
mouth.append(u"╭╮");
mouth.append(u"ロ");
mouth.append(u"_");
mouth.append(u"෴");
mouth.append(u"ꔢ");
mouth.append(u"⏠");
mouth.append(u"⏏");
mouth.append(u"益");

# COMPOUND FACE
auxEars = randint(0, len(ear1)-1);
auxEyes = randint(0, len(eye1)-1);
auxMouth = randint(0, len(mouth)-1);

social = "\n#gamedev #indiedev";
face = ear1[auxEars] + eye1[auxEyes] + mouth[auxMouth] + eye2[auxEyes] + ear2[auxEars]  ;

#Changes directory to where the script is located (easier cron scheduling, allows you to work with relative paths)
abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

def randomimagetwitt(folder):
    #Takes the folder where your images are as the input and twitts one random file.
    images = glob.glob(folder + "*")
    image_open = images[random.randint(0,len(images))-1]
    api.update_with_media(image_open, face)

#Twitts
randomimagetwitt("images/")