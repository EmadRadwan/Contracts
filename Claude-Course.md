Chapter 1: Introduction to the Claude Code Tutorial
0:000 secondsHey, this is the definitive course on Claude Code for beginners. I use Claude Code every day to manage a business that does over $4 million a year in profit.
0:077 secondsI also teach over 2,000 people how to use Claude Code, both for personal and then corporate or professional tasks. So this is more or less what I do all day.
0:1515 secondsOnce you understand what I'm about to show you in this course, it's no small stretch to say that Claude Code will augment your productivity.
0:2121 secondsYou'll gain leverage in areas that you probably didn't even realize that you had, and that's both for software engineering and also other parts of your life.
0:2828 secondsThe focus here is not software per se, so you don't need to have a technical background to understand what I'm going to tell you.
0:3434 secondsI'll make sure to start slow and build concepts on each other naturally and gradually so that everybody here is on the same page. So no fluff, here's what you guys are going to learn in this course.
0:4242 secondsWe're going to start with the basics by downloading and then setting up Claude Code ourselves. I'll then teach you all about integrated development environments or IDEs.
0:5050 secondsThere's several on the market, and I'm going to walk you guys through the three most commonly used ones so that we're all on the same page.
0:5656 secondsAfterwards, I'll show you how to set up your project brain, which is also known as the cloud.md file.
1:011 minute, 1 secondOnce we're done with that, we'll use Claude Code to actually build something because the focus of this whole course is on practical building.
Chapter 2: Learning the Basics of Claude Code
1:081 minute, 8 secondsWe'll build a simple web app hosted live on the internet, which I hope you guys learn by doing, not just sitting around and listening to me.
1:141 minute, 14 secondsAfter that, we'll cover the cloud directory, the subagents folder, and a bunch of functionality that not a lot of people know about.
1:201 minute, 20 secondsWe'll then cover Claude Codes, various modes, including their plan mode, which you guys might have heard about, dangerously skip permissions mode, which gained a fair amount of notoriety recently, and how to use them, as well as use them safely.
1:321 minute, 32 secondsWe'll then cover complex project builds using plan mode and what I just showed you guys. After that, we'll cover context management, which is quite the term right now.
1:401 minute, 40 secondsI'll teach you guys all about how to manage your context efficiently, avoid context rot, and ensure that your prompts are built and structured in a high ROI way.
1:471 minute, 47 secondsI'll run you through every slash command and Claude Code and how to use all of them.
1:501 minute, 50 secondsWe'll then cover hooks, which are custom scripts that you guys can fire automatically before or after every Claude Code tool call. Very useful to know.
1:571 minute, 57 secondsI'll then talk about Claude Code skills, which is basically how to create these skill files that turn Claude Code into a bunch of different specialized agents.
2:032 minutes, 3 secondsWe'll then cover model context protocol and how to set it up effectively.
2:072 minutes, 7 secondsI'll talk about a handful of automated systems that you guys can quickly build with model context protocol, including email managers. You can build your own bookkeeper and more.
2:152 minutes, 15 secondsI'll cover Claude Code plugins and marketplaces, the Chrome DevTools integration, which is on a connection between Claude Code and Chrome that enables you to collect data from sources that don't have APIs.
2:262 minutes, 26 secondsIt's very, very valuable to learn. We'll then cover Claude Code subagents with scoped tool access.
2:312 minutes, 31 secondsI'll talk about their new agent team feature and how to use them productively and then get work trees and session mobility, which essentially will allow you to spin up parallel
2:392 minutes, 39 secondscloud sessions without a lot of the downsides and issues that things like a cloud bot or open cloud have unfortunately resulted in.
2:462 minutes, 46 secondsFinally, we'll cover scaling and deployment.
2:482 minutes, 48 secondsBasically, how to take your automations and run them in production using modal webhooks, GitHub actions and Claude Code on the web. So we've got quite a lot to cover.
2:562 minutes, 56 secondsLet's just dive right into it with the first, which is how to set up Claude Code as a total beginner. So the first thing we have to do is we actually have to purchase Claude Code.
3:033 minutes, 3 secondsAnd the reason why is because they don't offer it for their free plan at zero dollars.
3:073 minutes, 7 secondsIn order to have access to Claude Code, you need at least their pro plan for everyday productivity. I'd recommend this if you guys are starting out.
3:143 minutes, 14 secondsThe money that you spend on a subscription like this is.
3:193 minutes, 19 secondsSo small compared to the massive productivity benefits that despite the fact that at 17 dollars, I personally would not even raise an eyebrow.
3:273 minutes, 27 secondsIt's no small stretch to say that Claude Code probably delivers me productivity benefits on the order of 10 to 15 thousand dollars a month, because it's not only just skilled
3:363 minutes, 36 secondsas a developer might be, which allows me to build systems that alleviate stresses and strain in my life, but it's much more than a developer as well. It's basically my second brain at this point.
Chapter 3: Setting Up Claude Code
3:453 minutes, 45 secondsAfter you click, try Claude, it'll take you to a page where you have to log in. And once you're done, you can then create your account for the very first time. So I'd select both of these.
3:533 minutes, 53 secondsI'm not going to subscribe to occasional product updates because my email inbox is busy enough. And then you have an onboarding screen with some personal information. So I'm just going to fill that out.
4:014 minutes, 1 secondAnd then once I'm done, circle back. OK, so I'm Canadian. And unfortunately, our dollars convert quite poorly to freedom dollars.
4:084 minutes, 8 secondsSo the 17 dollars that we saw earlier is 28 dollars in my own currency. I'm going to click Get Pro Plan and then walk through the payment details below.
4:164 minutes, 16 secondsCool. And now I have a Claude subscription. This is all that you need in order to get set up. Everything else is totally free.
4:214 minutes, 21 secondsFrom here on out, the simplest way to get up and running with Claude code is just opening up a terminal instance. That'll seem pretty intimidating to a lot of you. So I'm not just going to show you how to do it in the terminal.
4:304 minutes, 30 secondsI'm also going to show you how to do it using what's called their graphical user interface, which they put together four or five months ago.
4:364 minutes, 36 secondsAny resource that I show you throughout this course is probably going to look a little different by the time that you look at it versus when I'm looking at it.
4:434 minutes, 43 secondsAnd that's because Claude code, Anthropic and just AI tools in general change really quickly, especially since most of the developers are also using Claude code.
4:504 minutes, 50 secondsSo it kind of multiplies the productivity here. What's important is not the specific layout, the colors, the words on the screen. What's more important is that you just know how to find it.
4:594 minutes, 59 secondsAnd so the number one resource that I personally use to look up advanced Claude code features is in the Claude code documentation.
5:065 minutes, 6 secondsIt's at code dot Claude dot com slash docs, whatever language you speak, just pump in there and then it'll automatically translate that over to.
5:145 minutes, 14 secondsSo the Claude code docs specify that in order to install Claude code in your system for the first time, you can run what's called a curl command here.
5:215 minutes, 21 secondsIf you're running a Windows PowerShell, you know, you can run this Windows CMD. You can run that just so we're all on the same page here.
5:285 minutes, 28 secondsWhen you have little snippets of text like this, what they're telling you to do is basically to open up a terminal or a command prompt.
5:355 minutes, 35 secondsSo on Mac OS, Linux or WSL, which are all different operating systems, in order to open up a terminal, you just type terminal.
5:425 minutes, 42 secondsWhen you do so, you then get a terminal.
5:445 minutes, 44 secondsNow, this terminal might look a little intimidating to you if it's your first time ever using something like that, but don't worry about it too much. I just wanted to show you guys how easy it is to get set up with Claude code in this.
5:535 minutes, 53 secondsAnd then afterwards, as mentioned, we'll do the graphical user interface stuff. OK, so this is what it looks like on Mac. If you guys are on a Windows, then you'll have to use the Windows key search bar.
6:026 minutes, 2 secondsThen to look up something like CMD or command prompt. At the end of it, you'll get something that looks pretty similar to this. From here on out, all we have to do is we have to copy over the command that it gives us.
6:116 minutes, 11 secondsSo because we want a native install and I'm in Mac OS, I'm just going to copy over this command. You can also click this little button over here and then alt tap back.
6:196 minutes, 19 secondsI'm going to paste it in and press enter from here on out. A bunch of complicated things are going to occur. If you don't already have it installed, may take you a little bit longer.
6:266 minutes, 26 secondsBut now we're good to go. Claude code is installed on our computer.
6:306 minutes, 30 secondsOnce you're done with all that, all you have to do in order to use Claude is just type the word Claude directly into your terminal. It's really that easy.
6:366 minutes, 36 secondsNow, if it's the very first time that you're logging in, you'll also have to authenticate. And it'll ask you to do so automatically when you open this stuff up.
6:436 minutes, 43 secondsIf not, you can also type backslash L-O-G-I-N.
6:486 minutes, 48 secondsOnce you click this, it'll tell you Claude code can be used with your Claude subscription or build based on API usage through your console account. How would you like to set up?
6:576 minutes, 57 secondsNow, in our case, we're using the cheapest, most effective method, which is the Pro, Max, Team or Enterprise subscription. It's also the most straightforward, which is why it's the one that I used in this course.
7:077 minutes, 7 secondsI'm just going to click Enter.
7:087 minutes, 8 secondsAnd then it'll then log you into your Claude account, the one that you just set up a moment ago. Once we're done, you're all set up for Claude code.
7:157 minutes, 15 secondsYou can close this window, then Alt Tab back. And you'll see that it's going to say just press Enter to continue.
7:217 minutes, 21 secondsNow, just so we're all on the same page here, all we've really done so far is we've just opened up a chat interface with an AI model.
7:277 minutes, 27 secondsIt's just instead of it being in like a nice desktop application or on the web, it's in our terminal.
7:327 minutes, 32 secondsAnd the value here is instead of running an AI model on the web or in some distant cloud server, what we're doing now is we're running it locally on our computer.
7:397 minutes, 39 secondsSo we actually have the ability to take this model and then locally modify files on our computer, write scripts,
7:487 minutes, 48 secondswrite stories, write poems, restructure our file organizer, clean up our PC or our Mac. Like this thing is currently connected to my computer.
7:567 minutes, 56 secondsAnd I'll run you guys through our permissions and all that stuff work later on in the course, as talked about in the outline. But even this alone makes it extraordinarily powerful.
Chapter 4: Terminal vs. Graphical User Interface
8:058 minutes, 5 secondsSo this screen can look pretty intimidating for beginners. Most people end up using the terminal flow, not the GUI flow. But I'm going to explain to you what you guys see here just for simplicity.
8:158 minutes, 15 secondsIn the top left hand corner, you have that cute little Claude code widget.
8:188 minutes, 18 secondsI think it's I don't know if it's supposed to be a crab or like a jellyfish, but it's adorable. Then you have Claude code and the actual version up above. Underneath, you have the model that you're currently using.
8:278 minutes, 27 secondsIn my case, I'm using Opus 4.6. Then you have the plan that you're on, in my case, Claude Mac. So this is a couple levels up from the pro plan. And then perhaps most importantly, you have the current working directory.
8:378 minutes, 37 secondsAs I mentioned to you a moment ago, this is working inside of your computer in a specific folder.
8:418 minutes, 41 secondsAnd so Claude code currently lives inside slash users slash Nick Sariah, which is basically like the home folder, at least on my Mac OS.
8:508 minutes, 50 secondsHere is your previous command.
8:518 minutes, 51 secondsAnd so I just wrote clear because I wanted to clear it all the way up and give you guys a fresh canvas. Here is where you actually insert the text.
9:009 minutesSo when you type stuff, it pops up underneath here, it tells you the model again. Then it gives you various modes. So in my mode right now, I'm in bypass permissions.
9:089 minutes, 8 secondsThis is sort of like a dangerous mode. It's a mode that not a lot of people feel super comfortable with.
9:129 minutes, 12 secondsBut it's the mode that I prefer for knowledge, work and intellectually valuable tasks. And I'll run you guys through more of that later on.
9:189 minutes, 18 secondsBut you can cycle through modes simply by clicking shift and tab, which I'll show you guys how to do. And then there's some additional information here.
9:259 minutes, 25 secondsThere's some version, the latest and then over here is, at least in my case, the token readout. And you know, it's really cool. You can actually adjust this.
9:329 minutes, 32 secondsThis sort of thing is your Claude code status line, which I'm also going to run you through. You can make it all colorful and all wonky and really fun. You can have it display whatever the heck you want.
9:409 minutes, 40 secondsSo the very first thing I'm going to do is I'm just going to say, hey, how's it going?
9:459 minutes, 45 secondsAnd immediately after, I'm going to take a screenshot so I can show you guys some more information.
9:499 minutes, 49 secondsSo opening this up in my drawing tool, what ended up happening is immediately after we said, hey, how's it going? You see that another prompt showed up called finagling.
9:579 minutes, 57 secondsThis is one of like a thousand different words that Claude code uses.
10:0010 minutesBasically, any time it's thinking, it's going to use some funny term like finagling or processing or, I don't know, rumpeting or considering or whatever the heck.
10:1110 minutes, 11 secondsThey're pretty funny. And the cool thing is you can customize that. Next, you have the number of seconds that your query has lasted. So I just said, hey, how's it going?
10:1810 minutes, 18 secondsAnd then two seconds in, it's now produced five tokens for me. And then finally, you also have the token count.
10:2410 minutes, 24 secondsSo just so we're all on the same page, a token is not the same as a word, but at least for the purposes of most of what you do, you can consider a token to be similar to a word.
10:3110 minutes, 31 secondsFor instance, I said, hey, how's it going? This is not one, two, three, four. This isn't four tokens. It might be four words. It's probably closer to six or seven tokens.
10:3910 minutes, 39 secondsBut just think about tokens as being analogous to words. Just a few more, if that makes sense.
10:4410 minutes, 44 secondsYou'll also see that an additional piece of information popped up down here called context. And this is really important. Context goes from zero to 100 percent.
10:5110 minutes, 51 secondsAnd that's how much basically conversation history you have in the current chat window with your current instance of Claude Code.
10:5810 minutes, 58 secondsThis becomes really important later when you're designing better context management techniques, which is a big portion of what this course is going to be all about,
11:0511 minutes, 5 secondsbecause at least as of the time of this recording, context management is sort of like the big bottleneck in getting these systems to do more and better for you.
11:1311 minutes, 13 secondsYou'll also notice that on the right hand side, my token counter, my status line here, it went up significantly.
11:1911 minutes, 19 secondsAnd so basically what this means is at about 20,000 tokens or so, or about 10 percent of the way through our entire conversation thread that's allotted to us.
11:2911 minutes, 29 secondsWhat's really cool is Claude Code will take all of that history and at regular intervals, it'll actually compress that for you by increasing the information density, taking a string of text
11:3811 minutes, 38 secondsand then making it higher information density and higher information density and higher information density successively.
11:4311 minutes, 43 secondsSo that even if you wrote something in kind of like a bloated way, a way that you could have used fewer words to say, as your context goes up and longer, cloud will automatically manage that for you to ensure that you're within the window.
11:5511 minutes, 55 secondsSo that's how to set up Claude Code in the terminal. Hopefully we're all on the same page.
11:5811 minutes, 58 secondsTerminals are really similar to graphical user interfaces, which I'm about to show you in a moment.
12:0212 minutes, 2 secondsI do recommend that you guys get used to using it in terminal, because when you use it in terminal, you basically unlock even more functionality. You can run a bunch of these side by side.
12:1012 minutes, 10 secondsYou could run different terminal tools and whatnot that give you guys faster refresh times. And we'll cover that sort of stuff later.
12:1612 minutes, 16 secondsBut what I want to do now is I want to show you guys how to run it in a graphical user interface.
12:2012 minutes, 20 secondsAnd these graphical user interfaces are typically managed by what's called an integrated development environment.
Chapter 5: Understanding IDEs
12:2412 minutes, 24 secondsWell, that takes us to the next logical question, which is Nick, what is an integrated development environment?
12:3012 minutes, 30 secondsAn integrated development environment, also termed IDE, is basically three things put together.
12:3712 minutes, 37 secondsOK, it's a file folder organizer plus a text editor.
12:4812 minutes, 48 secondsPlus an AI chat widget, similar to what you get if you go on chatgpd.com or cloud.ai.
12:5612 minutes, 56 secondsSo you know how on my Mac, if I go finder, I open up a basically series of folders where I can select different files and open them up and so on and so forth.
13:0513 minutes, 5 secondsYou can do the same thing on Windows. If you just type in folder or I don't know, the C drive or whatnot.
13:1013 minutes, 10 secondsWell, an IDE is basically that plus something like notepad or notes plus something like chatgpt all in one.
13:1813 minutes, 18 secondsAnd right now we have two major IDEs that the market is tending towards. The first is called Visual Studio Code, and the second is called Antigravity.
13:2813 minutes, 28 secondsVisual Studio Code is sort of like the OG one, because Antigravity is actually built on it. It was developed a lot longer by Microsoft. It's really, really extensible.
13:3713 minutes, 37 secondsIt has great support and it's very straightforward. So I'm going to show you guys how to set things up on it. But Antigravity, I would consider to basically be Visual Studio 2.0.
13:4613 minutes, 46 secondsSo not only does it have most of the same features now, although it is some of them are still kind of a little beta ish. It's also a lot more modern.
Chapter 6: Exploring Visual Studio Code
13:5413 minutes, 54 secondsAnd then there's a much bigger focus on AI, which is obviously kind of the whole point of this course. So I'm going to be showing you guys initially how to set things up in Visual Studio Code.
14:0214 minutes, 2 secondsThen I'm going to do Antigravity.
14:0414 minutes, 4 secondsAnd then for the rest of the course, we're just going to be doing all of our work inside of Antigravity. And Antigravity is really cool. There's some additional functionality within Antigravity, not even tied to Claude Code.
14:1114 minutes, 11 secondsSo the first thing we need to do is obviously we need to set up Visual Studio Code.
14:1514 minutes, 15 secondsIn order to do that, just head over to Visual Studio Code on Google over here and then download for whatever your specific application is. In my case, I'm downloading the Mac OS.
14:2514 minutes, 25 secondsI'm then going to have the download appear in the top right hand corner. I'm then going to give that a click and then go download unverified file.
14:3314 minutes, 33 secondsAnd then over here on a Mac, you again have to drag the little window over. So I'm just going to do that. And once you're done, you're going to have a page that looks something like this.
14:4014 minutes, 40 secondsSo remember earlier how I said it was like a file editor? Well, that's what this little left hand side is about. If I click open a folder, I can actually go through and I can open a folder on my computer.
14:4814 minutes, 48 secondsSo why don't I just go and open, I don't know, left click contact. OK, so now I'm inside of the left click contact folder.
14:5514 minutes, 55 secondsAnd you can see we have some files here, a gitignore, clod.nd, contact, index and a Netlify.toml. I'm going to go through all that sort of stuff in a moment.
15:0415 minutes, 4 secondsIt's not super important for now. But this is sort of like where the file explorer functionality comes in.
15:1015 minutes, 10 secondsIf I were to click on one of these, as you can see, we've now opened up a big text editor right in the middle of the screen. And so this is a bunch of CSS. It's a programming language.
15:1815 minutes, 18 secondsWhat's really cool is with clod coding, I actually have to know how to read any of this stuff. It'll just tell you everything. And so that is the text editor functionality. I can make changes.
15:2615 minutes, 26 secondsHey, what's up? You know, I could create a new file here if I wanted to called message.md.
15:3315 minutes, 33 secondsAnd I could say, hey, how's it going, YouTube? So just like in that way, we basically have file editing functionality.
15:4115 minutes, 41 secondsAnd then we can also select files to work on and stuff like that.
15:4415 minutes, 44 secondsAnd then on the right hand side, you have an agent tab, which is where you have your chat interface with AI. Now, right out of the gate, the VS code chat interface isn't actually clod code.
15:5315 minutes, 53 secondsIn order to access clod code, you have to download it as an extension. So I'm going to run you guys through that right now on the left hand side here. Click on these little blocks, then just type clod code.
16:0316 minutes, 3 secondsYou'll see a variety of these.
16:0516 minutes, 5 secondsThe one that you're looking for is the one that's developed by Anthropic, the one with that little check mark in it.
16:0916 minutes, 9 secondsBe very wary of downloading extensions that are not from official developers and vendors like Anthropic, simply because people have been known to insert malware and different things like that in these.
16:1916 minutes, 19 secondsSo very important that you're that you preferentially use verified sources.
16:2216 minutes, 22 secondsIn my case, I've already installed this, but all you have to do is go through that little installation wizard here. And then once you're done, you will have access to clod code.
16:2916 minutes, 29 secondsThe question is, OK, I have access to clod code. How do I actually use it? Well, it's really easy.
16:3316 minutes, 33 secondsIf you just go to the top right hand corner of the Citadel agent window, you now can just click on clod code. But you'll also see that there's a clod logo up here as well.
16:4216 minutes, 42 secondsWell, what the hell does this mean? If you click on this, you'll open up just like another window. And in my case, I open it up with the terminal default. So it's going to open up this in the terminal.
16:5016 minutes, 50 secondsThis could be pretty intimidating and kind of annoying, to be honest, juggling all these things. I'm just going to zoom in so it's easier for us to see. So my recommendation is at least for beginners, just stick to the one on the right.
17:0017 minutesThat one's simpler. And as you can see, it's a different user interface than the terminal. OK, so how exactly do you use this and what are all of the different features and buttons and stuff like that?
17:0917 minutes, 9 secondsCovering the interface, obviously, up top, you have the past conversations tab.
17:1317 minutes, 13 secondsAnd so as you build up more conversation history, you'll actually be able to jump back to any prior conversation you've had with Claude code over here. You can do that both locally and then on the Web.
17:2317 minutes, 23 secondsI don't have access to either of these yet because I just set this up fresh for you guys. Underneath that, you have the Claude code logo.
17:3017 minutes, 30 secondsUnderneath that, you have that cute little jellyfish or lobster, whatever the heck it is. Underneath, you have your little chat window. So here's where I can actually talk to Claude.
17:3817 minutes, 38 secondsHey, Claude, what's up? Once we open this, you'll see that similarly to how we had before, we have that little accomplishing fidgeting, whatever.
17:4817 minutes, 48 secondsWe have that little like process text come up. After that, you then have the response.
17:5317 minutes, 53 secondsThe response comes in in this little window, although you'll see it's different when it accesses files and stuff like that. Underneath at the very bottom left hand corner, you have the various permission modes.
18:0218 minutes, 2 secondsRemember how earlier, Mindset dangerously skipped permissions? We can do the same thing here. If you just click on this, you can cycle through all of the different possible modes.
18:1018 minutes, 10 secondsAnd you see that little window around the chatbot also changes.
18:1218 minutes, 12 secondsSo in my case, I'm asking before edits, which means because this is running locally on my computer, before it makes any changes to any local files, I'm going to say, hey, just ask me to make sure. Now, this is pretty safe.
18:2418 minutes, 24 secondsAnd a lot of people, especially coders and developers that are a little more old school, will usually work like this.
18:2918 minutes, 29 secondsBut personally, given that when Claude's really in the thick of things, it's asking me for edits every five fricking seconds.
18:3518 minutes, 35 secondsIf you really want to unlock that productivity, as I talked about before, you either use edit automatically or use bypass permissions. And I'll cover plan mode and whatnot later as well.
18:4418 minutes, 44 secondsTo the right of that is, and this is kind of intimidating for some people to understand, but this is the file that is currently being fed in as context.
18:5218 minutes, 52 secondsSo, for instance, do you see how here it says index.html? And if I click this, I get this little eye icon. Well, if I leave this open, basically Claude is currently looking at this file.
19:0219 minutes, 2 secondsSo what file are you looking at right now? It'll now tell me that it's looking through the index.html that's open in my editor.
Chapter 7: Getting Started with Antigravity
19:1219 minutes, 12 secondsIt hasn't read through the contents yet because reading through the contents of this massive file would feed a fair amount of tokens into context, which would charge me a fair amount of money. So right now it's not doing any of that.
19:2119 minutes, 21 secondsBut suffice to say, I can actually edit this in real time. Yes, change the title to Nick's YouTube example.
19:2919 minutes, 29 secondsAnd what it's going to do is it's going to go through my file. It's going to find the title, which is listed right over here. And then it's going to change that for me.
19:3719 minutes, 37 secondsThis is an example of a really simple, easy and straightforward change. But I could do way more. I could refactor this whole thing from light dark mode to light mode.
19:4619 minutes, 46 secondsSo I'm actually going to ask it to do so. Refactor this index.html from dark mode to light mode. And if you don't know what this means, it's OK.
19:5319 minutes, 53 secondsBear with me. We're actually going to rebuild a whole app using Claude code and various design patterns in a moment. The first thing it'll do is it'll try planning out the changes that it's going to make.
20:0320 minutes, 3 secondsAnd so it's doing a bunch of programmatic adjacent things right now, like it's filtering out a bunch of different CSS snippets.
20:1120 minutes, 11 secondsIt's doing a fair amount of work here. And you don't need to be a programmer to understand what's going on. We basically now given this a task, it's deconstructing the task into a list of high level steps.
20:2020 minutes, 20 secondsThen it's going to go through and it's actually going to present this plan to me for me to say yes or no to.
20:2420 minutes, 24 secondsNow you'll notice that when I did this, in addition to the interface changing and now the colors being blue, in the bottom right hand corner, we now have sort of a little pause button.
20:3220 minutes, 32 secondsThis pause button is pretty important because it allows us to actually stop a Claude code execution in process, like while it's working.
20:3920 minutes, 39 secondsSo I could theoretically change this at any point in time. OK, and I can actually pause it and then maybe I could give it some more instructions or, I don't know, tell it to do something differently. So I'm actually going to click this little button, then I'm going to go to bypass permissions.
20:5120 minutes, 51 secondsI'll say no plan, just do it. And what I've done is I've interrupted the process and the tool call, and now it's going to go through and instead of going to do this big fat plan, I'm just going to say it's the wild, wild west, buddy.
21:0321 minutes, 3 secondsJust get in there and start making some changes. When I did this, you'll notice that there's now a thinking tab that's open.
21:0821 minutes, 8 secondsIf you click on this, you can actually peer into the internal thoughts of Claude as it goes through and accomplishes your request.
21:1621 minutes, 16 secondsSo in this case, it said the user wants me to just refactor the dark mode to light mode without planning. Let me read the whole file, understand all the colors and then make the changes.
21:2421 minutes, 24 secondsAnd as you see, we just had some changes made, which is what this little blue thing is here, showing that we've made the changes.
21:3121 minutes, 31 secondsSo immediately after thinking, it then did some more thinking. Then down at the very bottom, it's now updated a bunch of the sections of my code and it's continuing down some little to-do list.
21:4121 minutes, 41 secondsSo this is how you interact with Claude code through the graphical user interface.
21:4421 minutes, 44 secondsAnd there are a couple of additional things, like you can click on this button to attach files and folders and use the browser. You can also check all of the commands here, which are pretty powerful stuff, and I'll cover them all in due time. So that's Claude code in Visual Studio code, VS code.
21:5821 minutes, 58 secondsLet's now cover how it looks in Anti-Gravity, how to set that up. And then immediately after, we're going to build an actual real web app using Claude code. As expected, Anti-Gravity is pretty similar.
22:0622 minutes, 6 secondsThey have a website here called anti-gravity.google. It's very sexy and clean. Wouldn't be surprised they built this with agents.
22:1322 minutes, 13 secondsYou just click download for whatever your specific operating system is, in my case, Mac OS with Apple Silicon. I'm going to give that a click. Then it'll go through that same process that we just did for VS code.
22:2222 minutes, 22 secondsOnce you open up Anti-Gravity, it looks very similar to what we just saw a moment ago with VS code. And that's because the two are sort of built on each other.
22:2922 minutes, 29 secondsSo just like VS code was both a file explorer, a file editor, a notepad and an agent manager. You can see here we have those three same ideas.
22:3922 minutes, 39 secondsOn the left hand side, we're going to have the folders. On the middle, we're going to be able to edit the text of the files that we work with. And then on the right hand side, we can actually talk to agents.
22:4722 minutes, 47 secondsFirst thing I'm going to do is I'll click open folder and we'll go back to, I don't know, left click contact, just so you guys could see what we're dealing with. And you'll understand here that the UX is just slightly different than what we had earlier.
22:5722 minutes, 57 secondsYou know, some things are indented. We have like some little cool symbols in the left hand side of the file. This isn't super important, but I just think Anti-Gravity looks cleaner, which is why I like using it.
23:0623 minutes, 6 secondsIn the middle here, if I click this index.html, you'll see that we also have the text pop up just like we did earlier. And the only real difference between Anti-Gravity and VS code is just what we have in this right hand side.
23:1623 minutes, 16 secondsEarlier, we could have used Claude code really easily because there was an actual dedicated Claude code button. Right now there isn't. In order to access Claude code, assuming that you've installed it.
23:2423 minutes, 24 secondsSo I had over here Claude code for VS code. Give that installation button a click. Assuming that we've installed it, we have to do instead is we have to double click somewhere here and then click on this little Claude icon.
23:3423 minutes, 34 secondsOkay. And then just delete the agent icon. And now you have the same layout that we had earlier in VS code. Just now you have it with Claude code.
23:4223 minutes, 42 secondsThe reason why is just because Anti-Gravity is a Google product. So they try and push the Google Gemini series of models. That's what we had on the right hand side earlier.
23:4823 minutes, 48 secondsAnd to be clear, this is a Claude code specific course, but you can also use whatever model you want to do, whatever purpose. Like the model type is less important than just the fact that you're really good at using it and the fact that it's smart.
23:5923 minutes, 59 secondsSo exact same layout here. Not going to cover it anymore. Let's get into actually building some stuff. So let's now build our very first app slash web page with Claude code.
Chapter 8: Building Your First Web App with Claude Code
24:1024 minutes, 10 secondsFor simplicity sake, I'm starting with probably the most straightforward build, which is just going to be a web page.
24:1724 minutes, 17 secondsAnd we're not just going to do the hero header, which is the top or above the fold section. We're going to do the whole website.
24:2324 minutes, 23 secondsAnd the reason why I'm starting with this is because I just want everybody to understand how good Claude code and similar tools have gotten at being able to design high quality websites.
24:3224 minutes, 32 secondsThis is a site up here called godly.website. And what it does is it basically just showcases really high quality design.
24:3924 minutes, 39 secondsAnd every single one of these, with maybe just a couple of exceptions, is now doable in probably, I want to say less than 10 minutes or so, front to back using Claude code.
24:5124 minutes, 51 secondsAnd this isn't me just pretending. This is something that I have done myself dozens of times. I've built really high quality websites.
24:5824 minutes, 58 secondsThe other day I built like 15 or so for a project. They all look just like this. So award winning design, award winning app functionality and stuff like that.
25:0725 minutes, 7 secondsThese are just a few of the things that you guys are going to learn today. In addition, you're also going to learn how Claude.md, which is the system brain file, affects your prompts.
25:1725 minutes, 17 secondsI'm going to run you guys through the three major ways that people currently design sites and the various ways that you guys could use these approaches to both design websites, apps, and more or less anything else you want.
25:2825 minutes, 28 secondsAnd then I'm also going to talk a little bit about deploying. So let's start with Claude.md.
25:3325 minutes, 33 secondsI have open in antigravity here, the same workspace that we were looking at before, with just a couple of changes.
25:4025 minutes, 40 secondsNamely, there's this node underscore modules folder here, which you guys don't have to pay attention to. This is automatically generated by Claude code every time we use a library or use some sort of NPM package.
25:5225 minutes, 52 secondsAnd then underneath we have Claude.md. Now Claude.md, as mentioned, is the brain of your workspace.
25:5925 minutes, 59 secondsTo make it really, really simple and straightforward for you, because I think a lot of people misunderstand how Claude.mds work, let's just look at a hypothetical conversation.
26:0826 minutes, 8 secondsOver here, let's say you are on the right hand side. And so what you do is you say, hey, research X for me.
26:2026 minutes, 20 secondsResearch, I don't know, the best trending posts on Twitter in my niche, whatever the heck.
26:2526 minutes, 25 secondsRight. And then what ends up happening is the model afterwards, Claude, whatever you're using, whether it's Opus 4.6 or 4.5 or Sonnet or Haiku, it'll respond to you in purple saying, share one moment.
26:3726 minutes, 37 secondsAfter it returns whatever you want, then you continue in this vein. And so what I'm trying to get at is there is a pattern here, right?
26:4526 minutes, 45 secondsThere's user and then there's model and then there's user and then there's model.
26:5126 minutes, 51 secondsThe way that the Claude.md prompt works is basically at the very first message, before you even get to that point, what's hidden from you is the fact that there's actually another prompt.
27:0427 minutes, 4 secondsOkay. This prompt is injected at the very top of your conversation string before you even send the first message.
27:1127 minutes, 11 secondsAnd so this Claude.md being sort of the very first thing that the model reads and sort of internalizes is really, really important to help steer the output of the ship.
27:2127 minutes, 21 secondsNow, what is steering the output of the ship? Well, I often use an analogy here.
27:2627 minutes, 26 secondsLet's say you're somewhere on the East coast of, you know, North America and you're trying to go to, I don't know, let's say the Westmost coast of Africa or something like that.
27:3627 minutes, 36 secondsAs you guys know, these intervening distances are really huge. These are, I don't actually know how long it is, but probably at least 10,000 kilometers or so.
27:4527 minutes, 45 secondsNow, if you're a ship positioned right over here, okay, and this is your port and your goal port is over here.
27:5327 minutes, 53 secondsLet's hypothetically say you have limited ability to steer the ship for whatever reason, the steering wheel or whatever the ship equivalent is, it just doesn't really turn that much.
28:0128 minutes, 1 secondWhat that means is if you wanted to make it as close as humanly possible to that X, what you would have to do logically is you'd have to make sure you're very, very accurate, at least when you leave the port.
28:1428 minutes, 14 secondsAnd the reason why is because if you're not, okay, if you give even a very slight range of possible, I want to say angles that you could go.
28:2328 minutes, 23 secondsOkay, it may not seem like that big of a difference if you go, you know, from this line to this line, at least initially, right?
28:3028 minutes, 30 secondsBut over an intervening distance of tens of thousands of kilometers, obviously this goes a very, very long way away from what your goal is.
28:3728 minutes, 37 secondsAnd so steerability in AI is basically when you try and minimize the number of potential or the width of all of the potential options.
28:4928 minutes, 49 secondsAnd so what Claude.MD does is it allows you to take the space of like, you know, a really wide angle of ways that the AI could go. Okay.
28:5728 minutes, 57 secondsAnd it's like, I don't even know where that we're going to go if we take that topmost path and then compress it down into a much more likely subset of possible options that the AI could go such that, you know, if you were to be even slightly off here, the impact on your final destination, while, you know, you wouldn't make it to your goal.
29:1729 minutes, 17 secondsYou'd still make it pretty close. So I want you to treat your Claude.MD as basically that initial trajectory that you launch all of your Claude sessions, whether in terminal or whether in the GUI tool, like I'm showing up here.
29:3129 minutes, 31 secondsSo with that understood, now that we're on the same page about how Claude.MD is injected at the very front of any conversation, you start to realize that there's a tremendous amount of value in making that Claude.MD as high quality as possible.
29:4329 minutes, 43 secondsOkay. So including a file, capital C-L-A-U-D-E dot M-D in any workspace project directory means that this is now injected at the front of our conversation.
29:5529 minutes, 55 secondsAnd so you don't talk to this any differently than you would Claude itself. This is just a file that standardizes it and makes it really easy to build in like conventions for different workspaces.
30:0530 minutes, 5 secondsIn such a file, you're going to want to be very concise, and you're also going to want to give it sort of the bounds of what this workspace is for.
30:1230 minutes, 12 secondsI could just as well actually copy this whole thing over, okay, and then paste this directly into my Claude code and then just get rid of my Claude.MD entirely.
30:2130 minutes, 21 secondsBut the value in having a Claude.MD is I just don't have to do that every time. It's initialized very top of that conversation history like we just saw.
Chapter 9: Utilizing the CLAUDE.md File (Project Brain)
30:3030 minutes, 30 secondsAnd so what's in here, to be honest, is not super important. I actually had another version of Claude just develop this based off some Twitter posts that I saw that talked all about how to build websites with best practices.
30:4130 minutes, 41 secondsAnd you guys have access to all this stuff down below. I obviously have that template folder that you guys could use to get this and anything else.
30:4830 minutes, 48 secondsBut suffice to say, this is how, or one of the ways rather, that you can currently design websites using Claude code.
30:5530 minutes, 55 secondsSo the three major ways that people are currently using Claude code and other agents to do designs are as follows.
31:0231 minutes, 2 secondsThe first is that you give it a pre-existing design and then you give it the ability to screenshot itself over and over and over and over again.
31:0931 minutes, 9 secondsAnd basically what happens is the first variant that they create that Claude code creates will be like an 80% match.
31:1631 minutes, 16 secondsThen it'll screenshot that, compare it directly to the source image and then list all the differences and then get 90% of the way there.
31:2431 minutes, 24 secondsAnd then it'll get 95% of the way there. And it usually can't get 100% of the way there, but it can get like 99% of the way.
31:3131 minutes, 31 secondsThe value in this sort of approach is what we're doing is we're basically taking an inspiration website. And so in our case, we're going to be using it on this site here.
31:3831 minutes, 38 secondsAnd then we're using that to template out a bunch of like design fundamentals. So like the size of the text, the colors, the way the buttons look and stuff.
31:4631 minutes, 46 secondsAnd then what you do is you just change the content of the site with Claude so that it's like whatever site you want it to make. So in my case, you know, I run this business called LeftClick.
31:5431 minutes, 54 secondsThis is my automation agency. You know, we help people install growth systems into their businesses, typically B2B agencies.
32:0132 minutes, 1 secondSo what I would do is I would basically try and rebuild this site using this design. And, you know, I could make some minor changes afterwards.
32:0732 minutes, 7 secondsBut so long as I start with this nugget, Claude tends to do a really good job afterwards. The second way to build is you basically just give it a massive voice transcript dump.
32:1632 minutes, 16 secondsFor those of you that didn't know, there are now ways for us to basically dump like a larger amount of text using a voice transcript tool. I'll show you guys what that looks like now.
32:2432 minutes, 24 secondsBut if I just hold this FN key, this little widget appears in the bottom of my screen. Now, this is listening to everything that I say.
32:2932 minutes, 29 secondsAnd because I can speak a lot faster than I can type, I can actually say a fair amount of pretty short period of time. Most people type between 50 to maybe 70 words a minute, but we talk closer to 200 words a minute.
32:3932 minutes, 39 secondsThat's a two and a half to maybe three X improvement. And because these models are so intelligent and smart and capable of extracting the meaning from the text, you know, text is all they look at all day long.
32:4932 minutes, 49 secondsWhat you could do is you could just use a massive voice transcript dump to basically spell out everything that you want on the website.
32:5632 minutes, 56 secondsThis isn't going to one shot your website because we don't have a pre-existing design. But then you can just go back and forth with it. And then in a fraction of the time of developing a real website, using a voice transcript tool, you can get pretty close.
33:0633 minutes, 6 secondsThe third major way people are currently designing is they use components.
33:1033 minutes, 10 secondsNow, for anyone here unsure of what components are, basically there are now services and tools out there like 21st.dev where designers have created specific components on websites.
33:2333 minutes, 23 secondsAnd there are features on these where you can actually click on it and then click on this button up here, copy prompt.
33:2833 minutes, 28 secondsOkay. And then it will take this entire web page, entire design, you know, this little animation flickering thing, this jump on a call button, the sign up here about whatever.
33:3833 minutes, 38 secondsAnd then it'll copy all the text needed to have Claude code reproduce that for you. And so it's really straightforward and simple. You just make an account of one of these services.
33:4733 minutes, 47 secondsAnd then let's say you're building a website, you scroll through and you're like, wow, I really like this background paths component, right? With these cool sweeping things. I want that on my website.
33:5533 minutes, 55 secondsYou just copy the prompt, paste it into Claude code and say, hey, install this thing somewhere up at the top. Because AI is great at language, you know, you can get pretty close.
34:0434 minutes, 4 secondsSo you can do all sorts of things with this. You could do like cool button borders. As we see here, you could have like a sign in component over here. You get a multiple cards.
34:1234 minutes, 12 secondsYou know, this stuff is okay. To be honest, I find it much easier just to go straight to number one, which is just giving it a design and a screenshot loop and just having it work off of something pre-existing.
34:2234 minutes, 22 secondsI don't want you guys to think of this as like you copying a design.
34:2534 minutes, 25 secondsAs you'll see, the end result will be quite different from this, but it's just a good way for you to like, get a rough idea of the end design and also not have to worry about things like the sizes of fonts, you know, the, the colors and so on and so forth.
34:3834 minutes, 38 secondsOkay. So we're basically going to use this as like our inspiration. And then once we have our inspiration in place, Claude's going to be able to design whatever we want, whether it's an app or a dashboard or whatnot, very, very quickly.
Chapter 10: Approaches to Website Design in CC +Antigravity
34:4834 minutes, 48 secondsThe final thing that I have to talk about before we actually do the designing is the difference between building something and then deploying. So when you build something, you're typically building it locally.
34:5734 minutes, 57 secondsWhen you do a tool, an automation, like we're going to do later on in the course or an app or a website, you know, we're running this thing on our local computer.
35:0535 minutes, 5 secondsBut if we want other people to be able to access it, then obviously we need to deploy it. We need to push it onto the internet and the variety of different tools that allow you to do so.
35:1235 minutes, 12 secondsSo today I'm just going to show you how to build the stuff. And then over the course, the next few modules, as we get deeper and deeper into your course, I'll also talk a little bit about tools like Netlify, Roussel, Modal and whatnot.
35:2235 minutes, 22 secondsThat allow you to pull to push both your software, the tools that you make, and then even things like websites and full fledged apps to the cloud so that other people can access it on a domain like, you know, Nick's awesome tool.com.
35:3535 minutes, 35 secondsOkay, so without further ado, how would I actually go about this design process?
35:3835 minutes, 38 secondsWell, as mentioned, I had this Claude.MD file set up here, and this is just something that I had Claude basically scrape through Twitter to find me the best practices of all of the different types of website designs out there that people are currently using Claude and other tools to create.
35:5135 minutes, 51 secondsAnd then I just had to like, write me a little script, basically a little summary. And this is very squarely this give it a design screenshot loop.
35:5835 minutes, 58 secondsIt's just written in like a very particular way. You do not need to know how the tools work. You don't need to know how anything works. You basically just need to know how to like, find a resource out there or use AI to find a resource and then use it to make your own Claude.MD.
36:0836 minutes, 8 secondsWith that in mind, what I'm going to do now is I'm actually just going to go on the website that I want. I'm going to screenshot it.
36:1336 minutes, 13 secondsHowever, if you guys aren't familiar, you know, if I just screenshot like one section of the site, like this, for instance, on Mac, then I feed it in. You know, I don't actually have most of the site, right?
36:2236 minutes, 22 secondsI only have that hero header. OK, in terms of how to actually build this puppy, use command shift I or right click on the page and then type inspect.
36:3236 minutes, 32 secondsThis will open up a window that looks something like this. Once you're done, change the dimensions to full page width on desktop.
36:3936 minutes, 39 secondsThat's usually 1920 by 1080. This is termed the widescreen aspect ratio. Then just hold command shift P. I think it's control shift P on Windows.
36:4836 minutes, 48 secondsYou'll open up this little command bar. With this command bar in place, you can then just type in screenshot and then go capture full size screenshot.
36:5536 minutes, 55 secondsIt'll actually scroll through the whole site and take an entire screenshot for you. If I click on this button now, as you guys could see, we now have a screenshot of the entire website, top to bottom.
37:0437 minutes, 4 secondsIt's kind of a hack. Not a lot of people realize that you can do this, but you can. It's pretty neat. And once we have this, we just have to do one more thing. It's pretty big right now.
37:1137 minutes, 11 secondsIf you were to send Claude Code, you know, like 20 megabytes or something like that, a file, number one, it would like really massively eat up your token limits.
37:1837 minutes, 18 secondsAnd then two, I think the API might have like a limit on this. We just have to make this file significantly smaller.
37:2437 minutes, 24 secondsSo I'm just going to open up this resize PNG file here, a page called resize PNG from I love image.com. You can use whatever the heck you want.
37:3237 minutes, 32 secondsAnd I'm just going to drag and drop this in. I don't know. 50% smaller, even like 75% smaller and then click resize images. This is now going to basically re-map this for us.
37:4237 minutes, 42 secondsYou can click download. What we're looking for is we're looking to get a file that's less than about. I want to say, I think like four or five megabytes or so.
37:5137 minutes, 51 secondsSo it's not perfect. Okay. It's a little bit blurry, but it's all right. I'm just going to go back and resize this one more time so that it's, I don't know, maybe a little bit bigger.
37:5937 minutes, 59 secondsLet's do 50% smaller instead of 75%. Okay. Once we're done, we can click download resized images. This one is about four megabytes or so.
38:0638 minutes, 6 secondsIf we open it up, you could see that it's still high quality, but it's much, much smaller than the other file, which is like three or four times. And now that we're done, we just add this into Claude Code.
38:1438 minutes, 14 secondsSo back to our Claude Code instance. I'm going to go down here to bypass permissions. Then I just need to go find the file.
38:2038 minutes, 20 secondsSo I'm going to click this top right hand corner and I'm just going to see if I can drag this in directly. So it's going to open this up. That's okay.
38:2838 minutes, 28 secondsJust zoom in, copy, and then you can actually paste this in directly. Okay.
38:3438 minutes, 34 secondsSo just click that copy button, paste it, and then you actually have the whole file as context. Okay. And then we just have to do one more thing. We're just going to head back to the website.
38:4138 minutes, 41 secondsI'm going to find actual and then scroll down to this little body tag and then right click and press copy styles.
38:4838 minutes, 48 secondsThis is going to copy the styles of the site, including the button colors and sort of like the little gradients in the background and so on and so forth. You can paste that in. Okay.
38:5738 minutes, 57 secondsAnd then I'm just going to press enter. Now that we've uploaded these, keep in mind that despite the fact that this might mean nothing to you or I, keep in mind that there's that extra prompt that's been injected up at the top.
39:0939 minutes, 9 secondsThat literally says when the user provides a reference image screenshot and optionally some CSS classes or style notes, you should generate a website. So that's what it's doing immediately.
39:1839 minutes, 18 secondsIt's analyzing the reference image and building this website recreation. Let me start by creating the actual HTML file.
39:2439 minutes, 24 secondsSo this will now walk through its own little to-do list, take screenshots of its created website, compare it with round one, basically do the same thing over and over and over and over again, until it gets to where we want it to go.
39:3539 minutes, 35 secondsAnd this is really what I'd consider to be the core building philosophy for Claude code.
39:4139 minutes, 41 secondsWhat you do is you basically give it a high level task, which in our case we did with the Claude.MD.
39:4739 minutes, 47 secondsThen we allow it to do the task and then we allow it to verify or basically judge its results.
39:5639 minutes, 56 secondsI think the reason why a lot of people end up sucking at Claude code or maybe they end up giving it instructions and then not being satisfied with those results is they'll just give it the task and then it'll do the task.
40:0740 minutes, 7 secondsAnd then their loop is kind of like this, right? Task, do the task, give it another task, do the task, so on and so on and so forth.
40:1340 minutes, 13 secondsIf you don't give Claude code the ability to verify its own results, either visually through a screenshot tool, or if you're building some sort of software through like automated testing mechanisms and so on and so forth, test driven development, then you lose like the vast majority of the value of AI.
40:2740 minutes, 27 secondsThe reality is AI is not going to be perfect the very first time, but the value of AI is not in its ability to one shot everything 100%.
40:3540 minutes, 35 secondsThe value of AI is its speed because you can have it get to 80%. Let's say this is like a, I don't know, a little quality bar or something.
40:4540 minutes, 45 secondsYou know, what you can do is you can immediately, you know, it's not just going to be like, this is time step one, two, three. It's not just immediately going to be at 100%, right?
40:5340 minutes, 53 secondsThat's just, that's not what it does. It's not going to go from here to here in like two seconds, then be done. What it is going to do though, is it's very quickly going to start here.
41:0441 minutes, 4 secondsThen it'll go here. Then it'll go here. It'll go here. And then eventually after two or three or four time steps, it'll, it'll hit that 100%.
41:1341 minutes, 13 secondsAnd you know, we think that this is a really long period of time. Okay. But in reality, this is like five minutes.
41:1941 minutes, 19 secondsAnd if you contrast this with how long it would take a human to do that same, you know, approach, you know, humans will probably yet closer to 100% quality on their very first go.
41:2841 minutes, 28 secondsBut it's not going to be like a minute or two. What this is going to be is it's going to be like, I don't know, five hours. You know, we actually believe it or not, tend to be a lot more precise than these machines that we've built.
41:3741 minutes, 37 secondsWe can one shot things to a much greater degree than they can, but their ability to test and then retest and work really, really quickly orders of magnitude times faster than we do is the real value.
41:4641 minutes, 46 secondsAnd that's something that I don't think enough people talk about. So just make sure there's always a task, do the task and then verify the results loop somewhere in here and you'll be fine.
Chapter 11: Importance of Verification in AI
41:5341 minutes, 53 secondsNow heading back to our, um, plot code instance, you could see it's now actually completed the first round of its HTML.
42:0142 minutes, 1 secondNow it's, um, screenshotted it as well. And then it's basically comparing the screenshot to the work that it's generated. And with this, it's going to make minor changes.
42:1042 minutes, 10 secondsSo as you see, the very first thing it's done is it's replicated to get paid the same day by setting a payment link or the most flexible invoice in the planet with the buttons and so on and so forth. Okay.
42:1842 minutes, 18 secondsIt's also replicated that top section and it's used little placeholders here with these 160 times a hundred little buttons, even with like the right tilt and whatnot, because it doesn't have access to the images.
42:3042 minutes, 30 secondsIt then is, uh, you know, entering these little divs, right? It's even got this cool little post-it note, which is really cool. And then it even has the reviews.
42:3742 minutes, 37 secondsAnd so as sort of like rebuilding the design of this website, it's doing a really good job. And we're only a couple of minutes in what's cool too, is if you check out the thinking tab, you can see that it's gone through.
42:4642 minutes, 46 secondsIteratively every section of the site. Okay. And it's, um, you know, listing what it needs to do next.
42:5142 minutes, 51 secondsSo better decorative elements and hero, better floating band, fixing the blue dot positioning, improving the invoice cards with map thumbnails. I don't know what half of this stuff means, but to be honest for me, it's not super important.
43:0043 minutesJust because I want it to be a little bit special and then show you the parallel capacity of Claude Code. What I've done here is I've actually opened up another anti-gravity instance.
43:0843 minutes, 8 secondsAnd what I'm going to show you guys how to do is actually design multiple of these simultaneously. Once we built this test, this do test, and then verify loop over and over and over again, which we already have in our cloud.
43:1943 minutes, 19 secondsM D it's actually really easy to spin up multiple prompts and just have like 10 versions of cloud working on things simultaneously.
43:2643 minutes, 26 secondsSo just for shits and giggles, why don't we head back over to our little website designer? It's that giving me a file here called twin gate. Okay.
43:3243 minutes, 32 secondsAnd then I'm pasting it all in and then my computer's really humming. Like, uh, you guys probably can't hear this cause I like to noise cancel most things, but it's making some noise.
43:4143 minutes, 41 secondsAnd the reason why is because I now have two of these instances running simultaneously, both developing me a website on the left hand side of things. Just expand this.
43:4943 minutes, 49 secondsUm, we see that it's taken multiple screenshots, the screenshot one screenshot, two screenshot, three, you guys see how it's getting closer and closer and closer to the end result.
43:5843 minutes, 58 secondsWell, now it's doing some final editing. It's making some features thumbnails better on the right hand side. It's now going through the initial development of that new index.html.
44:0644 minutes, 6 secondsAnd so because you can run as many clod instances as you have tokens, basically, um, I can run as many of these website designers simultaneously in however many tabs I want.
44:1544 minutes, 15 secondsAnd this isn't even the most efficient way to do this. I'm going to show you guys a much more effective terminal management structure that'll allow you to do like five or 10 or 20 of these simultaneously.
44:2444 minutes, 24 secondsOkay. And the left hand side, it's now saying it's done. So I'm going to say open index.html. That's always just going to be the actual website file. And if you just tell it to open it, it's going to go through and do so in a tab for you.
44:3344 minutes, 33 secondsOkay. And here is the demo of the website that we put together. So, I mean, it's not perfect. It's not everything that I want, but it's good enough for us to start.
44:4044 minutes, 40 secondsSo what I'll do now is I'll go back and I'll have it recreate left click. Hey, this is looking pretty solid so far. I'd like you to check out left click dot AI.
44:4944 minutes, 49 secondsThat's my personal website. And what I want you to do is to design, uh, or take the information from left click dot AI and then insert it into this website.
44:5844 minutes, 58 secondsI don't want this to be a clone of left click dot AI, but I want it to be pretty close. Use the formatting and everything that you've developed so far to help place elements and stuff like that as necessary.
45:0845 minutes, 8 secondsUm, insert images as well and make sure that any elements that, um, are there look good. Continue doing a screenshot loop if necessary until you have something that looks very high end, very professional and minimalistic, just like you've already developed.
45:1945 minutes, 19 secondsOkay. So I just fed in a bunch of information. Now it's going to go through, fetch the content from left click, and then help me design the site.
45:2645 minutes, 26 secondsOn the right hand side, we're creating that initial index.html. Now in this case, I obviously did the two website design simultaneously manually.
45:3445 minutes, 34 secondsUh, but what you can do is you could actually work this into your website or app design process.
45:3845 minutes, 38 secondsYou'd actually have it take in, let's say three different examples of, uh, templates or of design inspirations, whether from godly.website or from, I don't know, dribble or one of these big design aggregators.
45:5145 minutes, 51 secondsAnd then in the cloud.md, you could say, Hey, actually, we need to develop three versions of this.
45:5545 minutes, 55 secondsThen you can give it some source and then you could actually just like, let it do its little test verification retry loop before giving it, you know, a source website, like in my case left, quick.ai to have it like do some modifications or maybe just doing a big voice dump of what your website is, what it's for the various audiences you serve and stuff.
46:1146 minutes, 11 secondsAnd then at the end, you could actually have three websites simultaneously that cloud presents to you after five or 10 minutes and says, which one do you like the best? The options here are virtually unlimited.
46:1946 minutes, 19 secondsThe other, uh, website developer so far has made this, which actually looks pretty reasonable. You could see that there's still some things that it needs to change.
46:2746 minutes, 27 secondsSome of the text looks like it's placed weirdly. Some of the blog posts and stuff like that. Obviously the development is mostly hands off at this point. I'm just monitoring it.
46:3446 minutes, 34 secondsAnd on the left-hand side, we've now taken four screenshots of this and gotten really, really close to that. And result, um, it's now building like the left click site itself.
46:4246 minutes, 42 secondsMost of the time, I don't actually care too much about what's in the file explorer. Um, so that is the third panel on the left-hand side of both of these windows.
46:5046 minutes, 50 secondsSo for simplicity, what I do is actually just close it out. And then I usually have on the right-hand side, some sort of output that AI has generated me.
46:5746 minutes, 57 secondsAnd then on the left-hand side, I just have my actual little chat window. I'm just going to zoom out just a tiny bit here. So we're still all on the same page. We could see everything.
47:0547 minutes, 5 secondsUh, and then that way I can now just orchestrate and kind of take a step back and see how things go. The left click design is also starting to come together.
47:1247 minutes, 12 secondsAs you could see, we've taken that initial website from actual as inspiration. So we have like the same sort of buttons and the nice rounding, nice hover effects on things.
47:2047 minutes, 20 secondsAnd then obviously we have the font. Uh, but then now we've actually replaced it with left click content.
47:2547 minutes, 25 secondsSo the definitive AI growth partner for fast moving B2B companies, tens of millions of dollars generated and more saved, proprietary systems, real revenue, no fluff. So we scroll through here.
47:3247 minutes, 32 secondsYou can see it's even inserted like a little a button and click video element. We all have our case studies down below.
47:3847 minutes, 38 secondsWe have some pictures of me and my business partner, although we're kind of cut off at the middle of the head, so we could probably fix that. And, uh, yeah, we've even got some testimonials, which is really, really clean.
47:4647 minutes, 46 secondsLet's see what happens if I click this button. Oh, nice. It's even gone to our discovery page.
47:5047 minutes, 50 secondsWe were actually like having button click functionality and stuff like that in here as well. Kind of curious what happens if I click on this. Okay. Nothing so far, but maybe I can tell it to do stuff.
47:5847 minutes, 58 secondsWe also have an about, and then we have a case studies. That's really nice. So yeah, I mean, things are progressing more or less exactly like we wanted them to. We even have our little logo from here on out.
48:0748 minutes, 7 secondsI'm just making minor changes and, um, you know, going to go back and forth with it until I get what I want. So on the left hand side, I'm just going to voice dump in my voice transcription tool.
48:1448 minutes, 14 secondsI can do this like this. I really like the output. I think the logo in the top left hand corner is a little too big. Make that smaller.
48:2348 minutes, 23 secondsThe bolding of the hero header font is also quite strong.
48:2748 minutes, 27 secondsSee if we could try a Sarah font instead of a sans Sarah font underneath the introducing left click section.
48:3448 minutes, 34 secondsWe have a button player over the picture of myself and Alex Ramose and Sam Evans. But when I click on this, nothing happens.
48:4348 minutes, 43 secondsEither turn this into a light box or eliminate that little button in the middle. The rest of these look great.
48:5148 minutes, 51 secondsMy and Noah's profile pictures are currently cut off at around the middle of our foreheads.
48:5648 minutes, 56 secondsSo move us down and zoom out of the photo slightly so that we're perfectly centered in frame and everything else here looks great.
49:0549 minutes, 5 secondsMeanwhile, on the right hand side, we see this index.html is now done so we can open this up. I'll say open in Chrome.
49:1349 minutes, 13 secondsThat's not going to open up the other version of that website for me. And it's looking like it's pretty clean. It's pretty matched with what we have.
49:1949 minutes, 19 secondsSo because I want to do the same thing that I did with the other source, I'm just going to scroll back up to where I gave it the instructions to basically copy over left click.
49:2749 minutes, 27 secondsAnd then I'm just going to paste this in. And now I have this also customizing the site to my specs. You don't have to develop in multiple tabs.
49:3549 minutes, 35 secondsThis is something that I think you learn how to do the more of these Claude code agents, frankly, that you're orchestrating.
49:4249 minutes, 42 secondsThe benefit to this is obviously you can develop basically however many times faster as tabs that you have open. But the downside is you also tend to context switch a fair bit.
49:5149 minutes, 51 secondsThe number one thing that you don't want Claude code to do is basically just sit around waiting for your instructions.
49:5649 minutes, 56 secondsSo if you are going to do it this way, just be honest with yourself and ask yourself whether or not there's always like Claude code operating in the background.
50:0350 minutes, 3 secondsI find if it's not running because it's waiting for you for more than maybe 10 or 20 percent of the time, you probably have too many tabs open. Personally, I cap out at about three or four.
50:1250 minutes, 12 secondsDepends on how intellectually heavy the things that I'm building are. And, you know, it's a learned skill. It's not something you're going to figure out right away. There's a fair amount of like remembering that you have to do as well.
50:2250 minutes, 22 secondsI've built a couple of things to help me build things faster. One of them is a little hook.
50:2550 minutes, 25 secondsThat's a chime that keeps on going off that you've probably been like, hey, what the heck is that thing? That's something that you can do. And I'll show you guys how to do a little bit later on in the course.
50:3350 minutes, 33 secondsWith that knowledge, you can basically set different chimes for different windows. And when chime one plays, for instance, you know that your top left window is done.
50:4150 minutes, 41 secondsSo you can go give it some more instructions, look at the results.
50:4350 minutes, 43 secondsWhen chime two plays, you know, you can go to the top right window and do some work there as well. All this stuff in due time. Okay.
50:5050 minutes, 50 secondsNow we've implemented all of the changes that I want, including some changes that I didn't even mention.
50:5550 minutes, 55 secondsSo you see here in the background, there's this very slight little vertical line design that it pulled from my main website, which is really clean. I like that. It makes it, makes it quite different.
51:0351 minutes, 3 secondsWe also have a serif font instead of a sans serif. I like that. Makes me stand out a bit.
51:0851 minutes, 8 secondsAs we scroll down, you can see that we've since removed that little play button, which didn't really make any sense. And it was looking clean. We have all of our profile photos. I like how it kind of inset us a bit.
51:1651 minutes, 16 secondsIt looks like my buddy Noah's still quite cut off, which is unfortunate.
51:1951 minutes, 19 secondsSo I'm going to have to fix that up, but the rest of this looks really good, which, uh, you know, I'm a fan of. Let me just make sure all these buttons work again. Cool. That goes directly to our thing with some minor changes.
51:2851 minutes, 28 secondsI think this website's basically ready to go and looking at the other option here. We've, um, more or less taken the same hero header. We have the calendar button working.
51:3751 minutes, 37 secondsYou have this nice noise background, which I like. We still have some issues with the photos and them being cut off.
51:4251 minutes, 42 secondsYou're going to get stuff like this pretty, pretty often, to be honest with the ad, but that's okay. You can also manually readjust them if necessary. I don't really like how there are two logos, so I'm just going to do the same thing.
51:5351 minutes, 53 secondsHey, this looks great. I don't like that there is both an image logo and then a text logo. Just have the text logo. We want to text Graham just called left click in the top left hand corner.
52:0452 minutes, 4 secondsThe noise background gradient looks a little bit blurry, so remove that. Only keep it on the social proof section.
52:1552 minutes, 15 secondsMyself and Noah's faces look fine.
52:1952 minutes, 19 secondsJust move Nick's or I have his head down about 15% as it's getting cut off a bit right now, center of the testimonials and client review section right now.
52:2952 minutes, 29 secondsIt's a little bit weirdly set off to the left and then change the 2025 copyright to 2026 that's all.
52:3752 minutes, 37 secondsAnd that looks a lot cleaner to me. We have our case studies nice and centered. Both of our heads are visible, which is really clean. We have our various services and then down here.
52:4552 minutes, 45 secondsLet me just click this button and make sure it opens that tab. Nice. So I mean, you know, I wasn't juggling this and trying to show you guys how to do it.
52:5152 minutes, 51 secondsRealistically, hopefully you guys could see, you could build your own super clean, high-end sexy website and probably less than five minutes now, at least locally.
52:5952 minutes, 59 secondsLater on in the course, I'm going to show you guys how to take this local website and then deploy it that will similarly just take a few minutes once you know what you're doing and the various platforms to use.
53:0753 minutes, 7 secondsSo you could take the same approach. You could use it to build an app. You could use it to build a dashboard.
53:1153 minutes, 11 secondsYou can use it to build more or less whatever you want, whether you are sourcing websites from a repository like godly.website or dot design or whatever, or you're doing this maybe a little more manually.
53:2253 minutes, 22 secondsMaybe you're actually going into apps that you really like and then you're using them as design inspo either way is perfectly fine.
53:2753 minutes, 27 secondsSo long as you start with that little nugget, everything else, as you guys see here, it gets a lot easier and worth noting.
53:3253 minutes, 32 secondsI just designed for desktop today, but if you want it to design for mobile or whatever, you do the exact same process. You would just do it with a mobile screenshot.
53:3953 minutes, 39 secondsIf you are just designing for a website and make sure that your websites are mobile and responsive and stuff like that, lest somebody open it up on their phone and get treated with my giant ass forehead.
53:4953 minutes, 49 secondsYou can also do that in the agent really easy. Just say, Hey, make sure this is nice and mobile optimized. I'm noticing XYZ images in a weird place. Okay.
53:5653 minutes, 56 secondsSo hopefully you guys have now learned at least a little bit about the way to do a practical build and practical design with Claude Code, as you see a lot of it's quite hands off.
54:0354 minutes, 3 secondsIt's not like extraordinarily involved. What you do is you basically steer it.
54:0754 minutes, 7 secondsLike I talked about before you carve out the river and then you just give it a boat and then it just goes along its way.
54:1454 minutes, 14 secondsSo long as there's some sort of test driven development loop, some sort of screenshot or verification loop, the quality that you can end up with is orders of magnitude better than not.
54:2254 minutes, 22 secondsAnd if you guys are ever wondering why you're not getting the results that you want, just make sure you have some sort of verification loop built in next up.
54:2754 minutes, 27 secondsWe're going to learn how to build significantly more complex tools, not just websites and visually designed things, but also whole backends, whole
54:3454 minutes, 34 secondsarchitectures and things that you could use either to, I don't know, like launch your own SaaS product or build really cool internal tooling for yourself, your own personal life, or for your team.
Chapter 12: Advanced Claude Code Functionality
54:4354 minutes, 43 secondsAll right.
54:4354 minutes, 43 secondsNow that we've done a little bit of building with Claude Code, we've put together what I would consider to be pretty solid websites with just a few moments of work, let's dive a little bit more into Claude Codes, advanced
54:5254 minutes, 52 secondsfunctionality, and I won't let you guys know that what I'm about to talk about here, probably less than 10% of everybody that currently uses Claude Code understands.
55:0055 minutesSo when you unlock what I'm going to be teaching you in this module, you'll know significantly more about Claude Code for one, and then you'll also be able to
55:0755 minutes, 7 secondscombine each of these cool different features in fantastic ways that I think you'll quickly see the value of. So what is the .cloud directory?
Chapter 13: Understanding the .claude Directory
55:1755 minutes, 17 secondsJust to be clear here for anybody that doesn't know in programming convention, first of all, this is a folder.
55:2355 minutes, 23 secondsAnd in programming convention, if you put a period in front of the folder, this basically hides the folder from view. And so if you just open it up in a file explore, you wouldn't actually see.
55:3255 minutes, 32 secondsFor instance, you know how like, I don't know, in my case, my computer is called Nick and then underneath that I might have some, some other folders, maybe I'll have like a documents or something.
55:4055 minutes, 40 secondsLet's turn this off before that frustrates me. I might have a documents. Well, if under Nick, I stored another folder called .hidden.
55:4855 minutes, 48 secondsIf I were to open up my file explorer, because it has a period in front of it, and because that just happens to be the convention of the file explorer wouldn't show it to me.
55:5555 minutes, 55 secondsSo this is sort of like the developer way of, you know, building folders that don't really muck around and ruin your nice organization.
56:0256 minutes, 2 secondsSo in Claude codes case, they have a dot lowercase C Claude directory.
56:0756 minutes, 7 secondsAnd inside of this Claude directory, there's basically support for like 10 or 15 cool advanced features that once you know, you can augment Claude code significantly more than sort of vanilla out of the box.
56:1856 minutes, 18 secondsSo let's run through all of them together. This is what like a fully loaded dot Claude folder would look like. Okay. And there's actually two levels to this and I'll cover both of them in a moment.
56:2756 minutes, 27 secondsBut the one that I want to talk about first is right over here.
56:3056 minutes, 30 secondsSo inside of the dot Claude folder, you can add a settings dot Jason, which is team permissions and hooks.
56:3856 minutes, 38 secondsI'll talk about hooks a little bit later on, but that's how I get my cool little chime noise at the end of everyone. You have settings dot local dot Jason.
56:4556 minutes, 45 secondsAnytime you have a dot local inside of a file, this basically keeps it local on your computer as opposed to push it, pushes it to a online repository.
56:5456 minutes, 54 secondsFor those of you that are unaware, a lot of programmers and people that use Claude code use GitHub to basically store all of their active projects.
57:0257 minutes, 2 secondsNow, because GitHub is a cloud service, there are some instances where you don't actually want the cloud service to have access to the data inside of your
57:0957 minutes, 9 secondsrepo, particularly if it's quite sensitive stuff like, you know, tokens and, and authentication keys and whatnot. So they develop this convention where you could just go dot local dot whatever.
57:1957 minutes, 19 secondsIn order to kind of override that and then not push it to GitHub. You have the same pattern here with Claude, where your Claude dot MD lives.
57:2757 minutes, 27 secondsAnd then Claude dot local dot MD. This is again, get ignored. That just means it's not going to go over to GitHub.
57:3357 minutes, 33 secondsThen interestingly, you have an agent's sub folder, you have a skills sub folder and you have a rules sub folder. And finally you have a hidden dot MCP dot Jason as well.
57:4357 minutes, 43 secondsYou know, I think if you're somebody coming into this without a technical background, you'd look at this and you'd like, be like, oh my God, this looks insane, like what the hell's going on settings dot JS.
57:5057 minutes, 50 secondsOh, and settings dot local dot JS. So when why is Claude capitalized, what does MD mean?
57:5557 minutes, 55 secondsAnd I'm going to explain all that stuff to you in due time, but for now, just know that these are basically the various buttons that Anthropec, the developers
58:0258 minutes, 2 secondsof Claude code have given you that you could press to sort of customize your own instance and each of these files, you can customize to whatever degree. You can add whatever the heck you want in there.
58:1158 minutes, 11 secondsSome of these files reference other files.
58:1358 minutes, 13 secondsYou know, it's really up to you and Claude, because most people don't actually develop this stuff on their own. Actually like kind of co-work with Claude to put together their own settings.
58:2158 minutes, 21 secondsBut it's up to you how intense you want to go into personally. I just have a Claude on MD.
58:2758 minutes, 27 secondsSometimes I'll have skills and agents and I'll run you through sort of like my own 80 20 setup later on in the course. Okay.
58:3358 minutes, 33 secondsSo anyway, this dot Claude folder actually lives inside of your Claude code folder workspace, wherever you're working.
58:4158 minutes, 41 secondsSo, I mean, I don't actually have a folder set up yet, but let me do it right now.
58:4558 minutes, 45 secondsAnd if you use this dot Claude folder, you're basically like unlocking advanced functionality more so than just having a Claude MD in the root of the folder.
58:5458 minutes, 54 secondsSo that's what I'm going to do.
58:5458 minutes, 54 secondsI'm just going to move over my dot Claude to, sorry, I'm going to move over my Claude MD to dot Claude, and then as you see, there are some additional folders here that I'm going to put together as well.
59:0459 minutes, 4 secondsInside of this, I'm going to go agents. Also going to go skills. And over here, I'm going to go rules and let's explain what all of these three mean.
59:1259 minutes, 12 secondsThe first idea is this idea of breaking up your big Claude MD into different rules.
59:1759 minutes, 17 secondsAnd so basically what this slash rules folder allows you to do is allows you to take everything that we've written here.
59:2459 minutes, 24 secondsAnd then instead of just sticking it all into one file, you can define high level rules that correspond to different parts of, let's say a build.
59:3259 minutes, 32 secondsSo for instance, in this example, there's a rule for code style. There's a rule for testing. There's a rule for security. There's a rule for front end.
59:4059 minutes, 40 secondsThere's a rule for, you know, within front end react and then styles as well.
59:4459 minutes, 44 secondsAnd so, you know, code style might be a very simple kind of two paragraph thing that just explains how to organize your code.
59:5259 minutes, 52 secondsSecurity might be a pretty simple few paragraph thing that explains how to, you know, secure your code basis and whatnot.
59:5859 minutes, 58 secondsStyles could be a list of Tailwind CSS styles or I don't know, whatever, just like some, some sort of formatting instructions to make websites look a certain way.
1:00:051 hour, 5 secondsAnd so for instance, if you look at our Claude MD over here on the right hand side, you can see that we've split it into a variety of sections already. There's like a workflow section. There's like a technical default section.
1:00:141 hour, 14 secondsThere's like a rule section. We can actually split these into their own rules files. And that's what I'm going to have Claude do in a second.
1:00:211 hour, 21 secondsSplit Claude.MD into its component rules. Use the Claude code rule specification if you don't know what that means.
1:00:311 hour, 31 secondsAnd so what I'm doing is I'm empowering Claude code to basically go through our current folder for one, then if it doesn't already know what, you know, rule specs are, it's going to go read up on rule specs.
1:00:421 hour, 42 secondsAnd then it's basically just going to take this file and then split it into, it looks like three file rules inside of the rules folder.
1:00:501 hour, 50 secondsSo now we have rules split into workflow, technical defaults, and then design rules. Okay. And as you could see, this is a little bit more compressed than we had earlier.
1:00:581 hour, 58 secondsBasically the title of the file is sort of like that little heading. Okay. Great.
1:01:031 hour, 1 minute, 3 secondsAnything else we'd need for efficient coding, you know, it can go through and it can create some additional rules for you.
1:01:121 hour, 1 minute, 12 secondsSo now if you think about it, okay. And by the way, I don't actually recommend just asking Claude, Hey, build me rules for efficient coding. It's not going to do a very good job.
1:01:201 hour, 1 minute, 20 secondsUsually the best place to find like high level instructions and stuff like that. That's sort of on the cutting edge.
1:01:241 hour, 1 minute, 24 secondsI would recommend like scrolling through Twitter and then finding Claude code power users. It's like a real gold mine. The reality is Claude code will actually
1:01:321 hour, 1 minute, 32 secondslike incorporate the most commonly used Claude done MD configurations and stuff like that into every successive generation. So a lot of the time, you know,
1:01:391 hour, 1 minute, 39 secondsyou don't have to include the stuff you had in your Claude and OD from like Opus four or whatever, because nowadays it just sort of understands that natively.
1:01:461 hour, 1 minute, 46 secondsAnd so if I, you know, talk about this example in the context of what we've already done, you know, over here we had one monolithic Claude done MD file, right?
1:01:541 hour, 1 minute, 54 secondsBut imagine that we instead split this into, I don't know, let's just say three rules. You know, we have the,
1:02:031 hour, 2 minutes, 3 secondsworkflows and then over here were the design rules and then the tech defaults. Okay.
1:02:121 hour, 2 minutes, 12 secondsNow instead of dumping it in as one big Claude and default, we actually have a lot more granular control over little things.
1:02:181 hour, 2 minutes, 18 secondsAnd so we can organize this to let's say evolve the workflow without touching the design roles and so on and so forth. And in general,
1:02:251 hour, 2 minutes, 25 secondsthis form of segmentation can be useful, especially when you're working with other people, you can give people access to let's say like the styles, but then maybe you actually have full control over like the top down workflow,
1:02:341 hour, 2 minutes, 34 secondsor as I'm sure you can imagine, you could have a really, really long Claude.md, right? A lot of people have Claude.md that are, I don't know, like many, many, many thousands of words, sometimes tens of thousands of words.
1:02:451 hour, 2 minutes, 45 secondsSo splitting it up in this way just helps keep you organized. It also helps allow you to see areas that like, you don't really need anymore. It's one thing if it's a giant file, it's 10,000 freaking words long.
1:02:541 hour, 2 minutes, 54 secondsIt's another thing if it's like pretty simple and pretty straightforward.
1:02:571 hour, 2 minutes, 57 secondsSo we can similarly create skills and agents and they're organized in very, you know, similar ways.
1:03:031 hour, 3 minutes, 3 secondsI'm going to talk through some specific agents that I'd recommend having, and then ways to use the skills folder to basically automate large portions of most knowledge work later. For now,
1:03:111 hour, 3 minutes, 11 secondsI want to talk a little bit about the top half of this image. So the bottom half, okay, this is stuff that we've already kind of discussed.
1:03:181 hour, 3 minutes, 18 secondsThis is the .Claude slash folder, but it turns out there was one folder that existed in even higher level than the .Claude in your workspace. Okay.
1:03:261 hour, 3 minutes, 26 secondsAnd this is like the global folder.
1:03:311 hour, 3 minutes, 31 secondsNow, anytime you see this little squiggle, okay, in computer programming or networking or in file in your file explorer, this basically refers to like your home folder. Okay.
1:03:411 hour, 3 minutes, 41 secondsAnd this isn't the home folder of your workspace, not the specific one that we're working on. This isn't, you know, if I go back to antigravity, my website design example, copy folder,
1:03:491 hour, 3 minutes, 49 secondswhat this is referring to is this is referring to like the home on your computer. And so this might be like the Nicholas folder or something like that on my computer.
1:03:571 hour, 3 minutes, 57 secondsAnd basically Claude code allows you to define settings that are both local, which corresponds specifically to the workspace that you're in and also global, which are basically settings that are shared between all of your workspaces.
1:04:101 hour, 4 minutes, 10 secondsAnd that's where the second sort of category bends into.
1:04:151 hour, 4 minutes, 15 secondsAnd so what we do is in addition to being able to set a Claude dot MD on the local level, for instance, AKA have one that applies to all workspaces.
1:04:231 hour, 4 minutes, 23 secondsIf we were to expand this just a little bit, the way that this thing actually works, if you think about it, is we have the Claude dot MD that's over here.
1:04:351 hour, 4 minutes, 35 secondsAnd this is your local dot Claude. Okay.
1:04:401 hour, 4 minutes, 40 secondsBut then we also have high level other Claude dot MD files and rules and stuff like that. Maybe this is called tech rules. Maybe this is called permissions.
1:04:531 hour, 4 minutes, 53 secondsYou know, maybe this one's called, I don't know, style guide.
1:04:571 hour, 4 minutes, 57 secondsAnd these come from your global little squiggly line slash dot Claude.
1:05:061 hour, 5 minutes, 6 secondsAnd the way that this is organized is very similar to the way that the local dot Claude is organized. It just exists in a different folder. And it basically supersedes any local dot Claude functionality.
1:05:161 hour, 5 minutes, 16 secondsSo this is another example of like splitting permissions. For instance, if you're working on a big team, you know, maybe you as the director of the team have access to like the global dot Claude
1:05:271 hour, 5 minutes, 27 secondstilde. It's called slash dot Claude folder. And in there, you put your like global settings.
1:05:321 hour, 5 minutes, 32 secondsSo these are high level rules at the age and in all workspaces reads and understands maybe things like, Hey, you know,
1:05:391 hour, 5 minutes, 39 secondsdon't allow people to delete these files or folders when speaking with, you know, staff members refer to them as X, Y, and Z, whatever.
1:05:471 hour, 5 minutes, 47 secondsAnd then every individual engineer on the team or every individual team member, they empower themselves with a local dot Claude folder.
1:05:531 hour, 5 minutes, 53 secondsAnd this is ways that a bunch of companies are currently starting to organize both their high level, you know, home Claude's or their global Claude's. And then,
1:06:001 hour, 6 minutesyou know, the ones that exist per workspace. So to make a long story short, there's actually three layers of Claude dot MD that merged together.
1:06:071 hour, 6 minutes, 7 secondsWe've talked about two of them so far, and there's like one more that's even higher level. But basically the first is your personal global.
1:06:141 hour, 6 minutes, 14 secondsAnd that is at the very top level here. That's in your home folder slash dot Claude slash Claude dot MD.
1:06:211 hour, 6 minutes, 21 secondsThen you have the per project or per workspace folder, which is dot Claude inside of your current workspace slash Claude dot MD.
1:06:281 hour, 6 minutes, 28 secondsAnd there's also a third option specifically for enterprise.
1:06:311 hour, 6 minutes, 31 secondsThis is like your managed system level Claude dot MD for enterprise licenses and stuff like that. 99.9% of you will not have enterprise licenses.
1:06:391 hour, 6 minutes, 39 secondsI'm not going to talk about this at all, but rest assured, it's a very similar concept.
1:06:411 hour, 6 minutes, 41 secondsYou just define another markdown file that, you know, sort of exists in that ranking or precedence level.
1:06:471 hour, 6 minutes, 47 secondsNow if I open up a repo that we haven't looked at before, this is my own left click site where I'm working using a strategy called get work
1:06:541 hour, 6 minutes, 54 secondstrees. Again, we'll chat about that later. But let's say, you know, I open up a new file folder and I want to run Claude code net.
1:06:591 hour, 6 minutes, 59 secondsAnd I don't actually have a pre-existing Claude code. And, you know, I want the model to help me with this. All I need to do is just open up that file folder, okay.
1:07:061 hour, 7 minutes, 6 secondsOpen up Claude code and then type slash in it. We'll get into more slash commands in a moment.
1:07:111 hour, 7 minutes, 11 secondsWhat this does is this basically allows us to analyze the current code base and then write a Claude dot MD that summarizes what the current code base
1:07:201 hour, 7 minutes, 20 secondsdoes and then give some instructions to, you know, a future version of Claude, which is really cool. So what this is doing right now is it's reading through all of the files.
1:07:291 hour, 7 minutes, 29 secondsIt's summarizing them. It's sort of looking through and, you know, seeing, uh, what, what stands out in the code base, trying to look for commonalities and patterns between them.
1:07:371 hour, 7 minutes, 37 secondsAnd then finally it ends up creating a capital Claude dot MD. And it does this directly in like the workspace route.
1:07:431 hour, 7 minutes, 43 secondsSo it doesn't do this inside of a dot Claude folder. You have to, you know, do this sort of organization yourself if you want to go any higher level.
1:07:481 hour, 7 minutes, 48 secondsBut as you can see here, it just put that together and I can open it up and I can actually see sort of like the way that it wrote its own Claude dot MD.
1:07:551 hour, 7 minutes, 55 secondsSo this file provides guidance to Claude code when working with code in this repository. This is a premium marketing website for LeftClack. It's an AI automation agency targeting to B2B companies.
1:08:031 hour, 8 minutes, 3 secondsHere's how to deploy it to Netlify. Here's the architecture. Here's the design system. Here's the Netlify config, et cetera.
1:08:101 hour, 8 minutes, 10 secondsWhy is this valuable? I mean, like it technically has access to all this information anyway, right? So like, why are we getting it to summarize it all?
1:08:161 hour, 8 minutes, 16 secondsWell, we're getting it to summarize it all because one thing we're going to talk a lot about in this course is context management.
1:08:201 hour, 8 minutes, 20 secondsAnd that basically just means all of the tokens currently in a prompt, as you've seen, there are multiple levels to this, right? There's like the global Claude dot MD that's injected.
1:08:291 hour, 8 minutes, 29 secondsThen there's the local Claude dot MD that's injected. There's the enterprise level Claude dot MD that's injected.
1:08:351 hour, 8 minutes, 35 secondsWe're then going to talk a lot more about the tool calls and various tool definitions. Those are all injected. And then finally, at the very end of it, you actually have your own prompt that you're sending,
1:08:431 hour, 8 minutes, 43 secondswhich is also part of the context. Well, if in addition to that, you force Claude to read through every single file, every time that you initialize to know what the hell you're talking about, obviously you have to add significantly more tokens to any prompt, right?
1:08:551 hour, 8 minutes, 55 secondsAnd by doing so a couple of things happen. One, the quality of Claude on average will go down because there's a negative relationship between the length of the prompt and then the quality of Claude's
1:09:051 hour, 9 minutes, 5 secondsoutputs. That's just sort of the way that it works statistically with these models. But two, you're also paying way more because now instead of consuming,
1:09:131 hour, 9 minutes, 13 secondsyou know, let's say 10,000 tokens at a time, you're consuming a hundred thousand, because this thing had to read through your contact dot HTML. It had to read your index dot HTML, had to read your message.
1:09:201 hour, 9 minutes, 20 secondsI mean, it had to read everything. And so Claude dot MD, if you think about it, in addition to providing high level instructions and, you know, uh,
1:09:281 hour, 9 minutes, 28 secondsand steering of the ship also is a mechanism by which you can significantly reduce your token usage and then increase the average quality of Claude,
1:09:381 hour, 9 minutes, 38 secondsbecause it'll just know everything, especially when you use a backslash in it, like I just showed you a moment ago, before actually having to read through the
1:09:451 hour, 9 minutes, 45 secondsfiles. You know, it'll know that index dot HTML uses an inverted light color scheme. Okay. It'll know that, you know, there's a contact dot HTML, which is a contact page. It'll know how it's hosted.
1:09:541 hour, 9 minutes, 54 secondsIt's not going to have to like do a bunch of API calls to various services to this out. It just already knows all this stuff because that's what this slash init just did. So if you don't already have a Claude dot MD, I'd highly recommend going to your folder, generate one.
1:10:061 hour, 10 minutes, 6 secondsOnce you have it generated, then you can continue making additions and changes as necessary, but literally just having a description of the way that the folder works is like,
1:10:131 hour, 10 minutes, 13 secondshonestly the, the, the 90% of the battle. So for simplicity, I've compiled the top recommendations into a quick do and don'ts guide for you.
1:10:211 hour, 10 minutes, 21 secondsThe first thing to do is just run backslash init first, anytime you're working in a new folder. The second is, I just use bullet points and short headings, try and compress information as much as possible.
1:10:301 hour, 10 minutes, 30 secondsBasically write in like a high information density style. Don't just voice transcript dumped into your Claude dot MD.
1:10:351 hour, 10 minutes, 35 secondsIf you wanted to write a Claude dot MD, for instance, using AS help, I might actually voice dump into Claude and then say, turn this into a very high information density summary of rules and stuff.
1:10:441 hour, 10 minutes, 44 secondsBut the most important things at the top, there's anything that like it absolutely shouldn't do like never delete XYZ file or whatever. Mention it up at the very top.
1:10:521 hour, 10 minutes, 52 secondsThe first few things that AI learns, it tends to remember. It's sort of like the middle gap of the prompt. If I were to show you guys what this actually looks like, basically goes like this.
1:11:011 hour, 11 minutes, 1 secondIt remembers a lot of the beginning.
1:11:031 hour, 11 minutes, 3 secondsIt doesn't really remember much of the middle and then it's more likely to remember some of the end. So this is called your primacy bias.
1:11:101 hour, 11 minutes, 10 secondsHuman beings are like this too, which is really interesting.
1:11:131 hour, 11 minutes, 13 secondsAnd then this is called your recency bias, which means, you know, Claude and us are biased towards remembering things at the very beginning of
1:11:201 hour, 11 minutes, 20 secondsa stretch and at the end of the stretch, but more so the beginning, which is why you put very important guardrails at the top. Periodically review and prune this. Like treat it like living code.
1:11:301 hour, 11 minutes, 30 secondsIf you have Claude constantly update the Claude.md, you will find over time it adds things that aren't really super necessary, some super precise instructions.
1:11:371 hour, 11 minutes, 37 secondsIt starts changing sort of the way that it talks to you and stuff. So I treat it sort of like technical debt and then I reduce it over time. What not to do is don't dump entire style guides and API docs into it.
1:11:471 hour, 11 minutes, 47 secondsThis is an unfortunate habit that I've seen a lot of people do where they basically are like, oh, you know, I want this to be my, I don't know, let's just say a Panda doc companion. So they go to the Panda doc API, then they download the entire thing.
1:11:571 hour, 11 minutes, 57 secondsAnd then they try and paste it into the Claude.md ends up being 10,000 tokens.
1:12:011 hour, 12 minutes, 1 secondAnd then keep in mind, this is initialized every single time you run Claude code, right? In addition to it taking a little bit longer, cause now you have that initialization time.
1:12:071 hour, 12 minutes, 7 secondsIt's also just a pain in the ass and it's, and it's more costly while reducing Claude's quality as mentioned.
1:12:121 hour, 12 minutes, 12 secondsSo don't do that instead, like talk to Claude, say, okay, what specific API endpoints are we going to need?
1:12:171 hour, 12 minutes, 17 secondsAnd then give it the whole API and then just have it like prune it down to just the specific sections that you need or specific, maybe high level instructions on how to use this API that maybe is not super relevant or trivial, I should say.
1:12:301 hour, 12 minutes, 30 secondsClaude.md allows you to do what's called an at include. This is very simple to just, you know, I didn't want to spend too much time on this,
1:12:371 hour, 12 minutes, 37 secondsbut basically if in your Claude.md, you just say, you know, at get.md and you have a folder called get.md somewhere else in your computer.
1:12:471 hour, 12 minutes, 47 secondsIt'll actually go and it'll like include that into the Claude.md.
1:12:501 hour, 12 minutes, 50 secondsAs you guys can see that functionality sort of taken care of by rules, but I just don't add include a bunch of files unless absolutely necessary.
1:12:581 hour, 12 minutes, 58 secondsDon't write really vague rules in general, like treat Claude like, you know, a really intelligent savant style intelligence, but also, you know,
1:13:061 hour, 13 minutes, 6 secondspeople that are, they tend to be really intelligent and savant take a really intelligent one specific little slice of the field. If you give them too much rope, they'll just hang themselves.
1:13:141 hour, 13 minutes, 14 secondsSo try not to write like really high level, vague aspirational things, unless absolutely necessary, unless it makes sense. For instance, don't just say be smart. Don't say make no mistakes.
1:13:241 hour, 13 minutes, 24 secondsClaude's not going to understand that. Right?
1:13:261 hour, 13 minutes, 26 secondsI keep seeing a meme rolling around Twitter and it's like, Claude, make me $1 million. Don't make any mistakes. And it's like, that's just not going to, that's not going to improve the quality of its output or anything like that.
1:13:371 hour, 13 minutes, 37 secondsIn general, you want to keep it somewhere between like 200 to maybe 500 lines or so max.
1:13:431 hour, 13 minutes, 43 secondsThe recommendation is not to go any longer than 500 lines. Otherwise again, you're just dumping it a ton of context. And then don't forget to add rules when Claude keeps making the same mistake.
1:13:521 hour, 13 minutes, 52 secondsSo like if you're working with a particular library or particular software platform, or again, a particular API, like Panda doc or whatever, and they have a very specific way of going about things, you know,
1:14:001 hour, 14 minutesevery time you load up a fresh instance of Claude code, it's going to continuously make that mistake, which is going to cost you again in tokens, but then also in context because of quality.
1:14:081 hour, 14 minutes, 8 secondsSo if you find that it makes a mistake more than two or three times, tell it, Hey, you know, I want you to add this to your Claude NMD so that this would work the next time I run it on a fresh instance of Claude.
1:14:171 hour, 14 minutes, 17 secondsThat's one of my favorite things to tell it. Okay. So these are just some high level rules. Obviously there are more.
1:14:221 hour, 14 minutes, 22 secondsIf you want like a really powerful way of, you know, finding solid Claude code tips and specifically at Claude.MD
1:14:301 hour, 14 minutes, 30 secondsstuff, I actually go straight over to Twitter slash X and then I say, you know, compile the last month of high ROI Claude.MD writings. What are the best things to include?
1:14:451 hour, 14 minutes, 45 secondsBecause this technology moves so quickly rather than me, you know, trying to like tell you guys to always include a certain snippet of text in your Claude.MD.
1:14:541 hour, 14 minutes, 54 secondsBasically I just have it go through the last month of Twitter posts. After a moment it'll tell you the most useful high ROI insights and patterns. Grok obviously is X's model. They have access to all Twitter posts.
1:15:031 hour, 15 minutes, 3 secondsAnd there's some extraordinarily intelligent people on here that basically live inside of Claude code. So I get most of like my advanced tips from them.
1:15:111 hour, 15 minutes, 11 secondsAnd yeah, you know, there's a lot of instructions and advice here given in just the last month or so. Okay. Now that we've talked about the Claude.MD,
1:15:171 hour, 15 minutes, 17 secondslet's talk about a few additional features that not a lot of people understand they have access to inside of Claude code.
1:15:241 hour, 15 minutes, 24 secondsThe first is this concept of auto memory. So basically in addition to the Claude.MD, there is an additional tiny little file that's injected at the top of every session.
1:15:341 hour, 15 minutes, 34 secondsAnd you'll find that Anthrobek and the developers of Claude code do a lot of these injections. It's not just the Claude.MD and it's not just this memory file, which I'll talk about. They have a lot of tool calls, definitions, lots of stuff.
1:15:451 hour, 15 minutes, 45 secondsSo the way that memory works basically is if you tell Claude something in one instance, and you tell it to remember it,
1:15:521 hour, 15 minutes, 52 secondsit'll actually write it to this memory file. And then in another instance, when you pull it up, this is like a global memory file. It'll, it'll remember you. So if I open up Claude code again and down here, I say,
1:16:021 hour, 16 minutes, 2 secondsI don't know, what's my brother's name.
1:16:081 hour, 16 minutes, 8 secondsSo I try and ask it some, let's say personal information that I want it to find out for me. It'll say, I don't know your brother's name.
1:16:151 hour, 16 minutes, 15 secondsYou haven't shared that with me. I say, remember that my brother's name is George.
1:16:221 hour, 16 minutes, 22 secondsNow what it's going to do is it'll save that to its memory file. Okay. Which already has a few other things like the fact that my dog's name is Yelpers.
1:16:311 hour, 16 minutes, 31 secondsYou guys think my dog's name is Yelpers?
1:16:341 hour, 16 minutes, 34 secondsThen if I go to a new fresh Claude code instance, and then I say, what's my brother's name? Notice how this time we're not going to have that issue.
1:16:411 hour, 16 minutes, 41 secondsIt's just going to say George. And the reason why, if we just go back to this very stereotypical, prototypical example, which just continues to grow.
1:16:491 hour, 16 minutes, 49 secondsIn addition to both the enterprise Claude.md, the global Claude.md and then the local Claude.md,
1:16:581 hour, 16 minutes, 58 secondsyou also have a file here, which is separate from all of those called memory.md.
1:17:091 hour, 17 minutes, 9 secondsAnd Claude will inject this at the very top of basically every new session.
1:17:141 hour, 17 minutes, 14 secondsSo in addition to again, this global section here, and then this local section, we also have a memory.
1:17:191 hour, 17 minutes, 19 secondsAnd then we have a bunch of other tool call definitions and stuff like that, which I'll talk a little bit about later in practice.
1:17:241 hour, 17 minutes, 24 secondsMemory isn't super valuable or anything like that. I mean, Claude.md does a lot of that, of course, but you know, it's separate from Claude.md.
1:17:321 hour, 17 minutes, 32 secondsYou can kind of treat this as Claude's own notes. It's not really your instruction set. Okay. Next up are agents.
1:17:371 hour, 17 minutes, 37 secondsAs you see here, we have this agent sub folder within the .Claude local settings folder. This can be pretty difficult to understand.
1:17:441 hour, 17 minutes, 44 secondsSo I'm just going to give you a high level overview now, and then we're actually going to do a lot more agent development later on in the course. But let's just say I want an agent called tell me the time.md. And this is a really simple agent.
1:17:581 hour, 17 minutes, 58 secondsI basically just want it to tell me the current time.
1:18:011 hour, 18 minutes, 1 secondI can define the tools that it has access to, the model, the max number of turns that I can have it autonomously go and fulfill my request.
1:18:091 hour, 18 minutes, 9 secondsWhether I want it to have global or local memory, I can give it a little description, a name, and then also down here, just like a brief little outline of what it is that I want to do.
1:18:181 hour, 18 minutes, 18 secondsAnd so in this case, hypothetically, I'm just saying this is a time teller.
1:18:211 hour, 18 minutes, 21 secondsYou know, I basically want my big agent to talk to my smaller agent and then say, Hey, what's the time? Very simple and straightforward.
1:18:271 hour, 18 minutes, 27 secondsSo I'm actually going to open up a new session here, and I'm going to say, what time is it? Use my agent.
1:18:361 hour, 18 minutes, 36 secondsAnd if you haven't already seen the sub agent tool call, it looks a little bit different from what you guys are probably used to. Notice how now we're opening up this task called tell me the current time.
1:18:461 hour, 18 minutes, 46 secondsAnd what happened is we see this little in input. What this is is this is our main agent talking to a sub agent.
1:18:531 hour, 18 minutes, 53 secondsAnd so this main agent basically said, I see that Nick said, what time is it? And he asked me to use my agent.
1:19:021 hour, 19 minutes, 2 secondsLet me check all of my available agents.
1:19:041 hour, 19 minutes, 4 secondsAnd then went through the agents folder, found that there was an agent called tell me the time.md and then said, Oh, I see there's an agent here that can tell me the time. Since Nick asked me for that.
1:19:141 hour, 19 minutes, 14 secondsThis is obviously the one that he wants me to use.
1:19:161 hour, 19 minutes, 16 secondsIt then creates a task called tell me the current time, and then sends the new agent a message saying, Hey, Nick wants to know the current time.
1:19:241 hour, 19 minutes, 24 secondsPlease determine the current time and report it back. Then at the very end, it says the current time is 2 23 PM empty. Anything else the agent wanted you to tell me?
1:19:371 hour, 19 minutes, 37 secondsYes. It greeted you with a howdy partner. And then it gave me a little cute cowboy emoji. The reason for that obviously is because down here I said also say howdy partner.
1:19:461 hour, 19 minutes, 46 secondsAnd so you can have agents for a million different things. In general, one off functions like tell me the time aren't really that valuable because, you know, your parent agent can sort of already tell you the time for the most part.
1:19:591 hour, 19 minutes, 59 secondsBut there are a couple of agents that do make sense.
1:20:021 hour, 20 minutes, 2 secondsAnd so if we split this into parents and then the variety of different ways you could call this used to be master slave, by the way, which, you know, had a bunch of issues. They had to change it.
1:20:101 hour, 20 minutes, 10 secondsNow it's like kind of like parent agent and then child agent. But if you think about it, there are a few agents that actually make sense.
1:20:191 hour, 20 minutes, 19 secondsThe first agent that makes sense is in general, having some sort of research sub agent.
1:20:241 hour, 20 minutes, 24 secondsThe reason why is because the way that agents work is they're spawned with their own context.
1:20:281 hour, 20 minutes, 28 secondsAnd so this agent down here that we just spawned has no no context aside from just this input. It literally the only text inside of it's, um, you know,
1:20:381 hour, 20 minutes, 38 secondsprompt is the user wants to know the current time, please determine the current time along with, you know, the high level instructions that we defined and tell me the time. Like that's, that's literally all that it has. That's its whole Claude done MD, essentially.
1:20:491 hour, 20 minutes, 49 secondsUm, and so because of this, because of the separation of context, you know, if you want to keep the total number of tokens that you use as low as possible in the parent agent, which is usually the smartest one,
1:20:581 hour, 20 minutes, 58 secondslike the one that you're paying the big API token usage and stuff like that for, uh, instead of trying to fill in a hundred thousand tokens of research when it goes
1:21:061 hour, 21 minutes, 6 secondson the internet and it looks up trends, then it goes, checks out Google analytics and then it goes pumps things into, I don't know, duck, duck, go instead of like filling or polluting all the context of the parent
1:21:161 hour, 21 minutes, 16 secondsagent. What you do is you basically just say, Hey, you know, go research X, Y, Z, and tell me
1:21:261 hour, 21 minutes, 26 secondsa summary. And then it will go pollute all of its own context window, get it super long might use 50 or a hundred thousand tokens,
1:21:341 hour, 21 minutes, 34 secondswhich is why a lot of people use the, uh, sonnet model series at the time of this recording for that purpose.
1:21:391 hour, 21 minutes, 39 secondsAnd then the only thing that actually makes it back to the parent is just that summary. So down here, this could use a hundred thousand tokens, right?
1:21:451 hour, 21 minutes, 45 secondsBut then like the tokens that it transmits back might only be, I don't know, like two K or something, which is, if you think about it at a cost savings amount of 50 times, we're literally 50 times cheaper than whatever the parent costs would have been.
1:21:561 hour, 21 minutes, 56 secondsAnd then we also get to use, um, you know, a lot of cheaper sub models and stuff like that, like sonnet, like Haiku and so on and so forth. So research is really, really good.
1:22:051 hour, 22 minutes, 5 secondsAnd that's one, uh, sub agent that I would almost always create.
1:22:071 hour, 22 minutes, 7 secondsI'm actually going to show you guys how to create one later for your code and then also for other automation purposes. Another one that I really recommend is basically having like a reviewer agent.
1:22:171 hour, 22 minutes, 17 secondsThe way that the reviewer agent works is in contrast with the research agent, you know, it having no context is actually the whole point.
1:22:251 hour, 22 minutes, 25 secondsSo basically what happens is this parent writes a bunch of code, right? You know, it's like your index.html or as we're going to see, it's going to be Python scripts or whatever the heck, it's just going to do a bunch of code for you.
1:22:341 hour, 22 minutes, 34 secondsAnd then after writing all that code, okay, its context is now really biased towards the way that it wrote that code.
1:22:411 hour, 22 minutes, 41 secondsBasically, you know, if you think about it, there's like 10,000 tokens and all of those tokens are like, Hey, you know, I should write the code this way because of whatever reason. Well,
1:22:491 hour, 22 minutes, 49 secondsif you want it to write really, really good code, a lot of the time what you have to do is you actually have to give it to another version of itself with no context and then just say, Hey, this is, this is the code knowing absolutely nothing. Do you think this is good code?
1:23:021 hour, 23 minutes, 2 secondsAnd if the answer to that question is yes, then obviously it's a good code.
1:23:041 hour, 23 minutes, 4 secondsBut if not, okay, what usually happens is when you do this, when you spawn a new agent and then give it the code, it'll say, it's kind of weird that you wrote it that way. Why did you write it that way?
1:23:121 hour, 23 minutes, 12 secondsAnd then the reason why is because the initial version of cloud, as mentioned, was just really biased because it had just done all this thinking and stuff like that. And so, you know, they do this in, you know,
1:23:221 hour, 23 minutes, 22 secondslike big enterprises, stuff like that. Like you do, what's called a code review where, you know, a programmer writes some big long function or some cool tool or creates a nice app.
1:23:291 hour, 23 minutes, 29 secondsAnd then they're so biased about the way to do things because they've just spent like 10 hours, you know, hammering a particular method or a particular approach that when they give it to
1:23:391 hour, 23 minutes, 39 secondsthe code reviewer, okay, another human being, the guy looks at it and he's like, what the hell is this? Why did you do it this way?
1:23:431 hour, 23 minutes, 43 secondsHe could have done it way easier with another way or whatever. Or, you know, Hey, I noticed that your security is kind of off. So in this instance, what the reviewer sub agent does is it basically takes advantage of the fact
1:23:531 hour, 23 minutes, 53 secondsthat it has no input and then it's able to look at the code with like a totally blank face. And so with this, you basically say, you know,
1:24:001 hour, 24 minuteslook at the code with zero context or no context and break down the code, plus improve it.
1:24:121 hour, 24 minutes, 12 secondsAnd then what it'll do is it'll take all of the code. So it might feed in, you know, like 10,000 lines or something. And then it'll return just the changes
1:24:201 hour, 24 minutes, 20 secondsto the parent agent. And then, you know, this might be again, like 2k tokens or something. And the parent agent will, will do the changes cause it's usually smarter. And then, you know, now your code's way higher quality.
1:24:291 hour, 24 minutes, 29 secondsFinally, one that a lot of people are using is sort of this middle one here, which is like QA slash testing. Now this is more of like an advanced programming
1:24:351 hour, 24 minutes, 35 secondsthing, but basically in order to determine whether or not a piece of code works or a tool works or a piece of software is like good, typically,
1:24:421 hour, 24 minutes, 42 secondsyou can develop a bunch of tests and then you can subject to your tool or software that you just created to these tests to figure it out. Now,
1:24:491 hour, 24 minutes, 49 secondsobviously your parent agent can do this, but, you know, this is just something that would pollute the context and be tremendously costly, both in terms of tokens, but also the intelligence of the parent model.
1:24:581 hour, 24 minutes, 58 secondsAnd so typically what people do is they'll break things down into this research sub agent, a reviewer sub agent, and then also some sort of QA or automated test sub agent in big enterprise. And that's how they do like automated testing of
1:25:081 hour, 25 minutes, 8 secondstheir code, automated test driven development, and so on and so forth, which is similar to what I was doing earlier when we designed those websites, where, you know, we tell it to do the thing, it goes and it does the thing, and then it uses some sort of way to verify that it did the thing correctly.
1:25:201 hour, 25 minutes, 20 secondsYou can kind of think of the QA agent as like a way to facilitate that.
1:25:231 hour, 25 minutes, 23 secondsIt's just with design, it's pretty easy because you just fee a screenshot in and then you look at, you know, the screenshot and if the screenshot's good, then you're good.
1:25:291 hour, 25 minutes, 29 secondsWith, you know, backend development, obviously you need a way to determine, hey, is the thing that I said that it should be able to do actually happening.
1:25:361 hour, 25 minutes, 36 secondsLast but not least, we have skills, which were previously referred to as custom slash commands.
1:25:411 hour, 25 minutes, 41 secondsNow skills are pretty great skills, basically allow you to automate a vast majority of, I want to say like the day-to-day knowledge work that you may or may not be
1:25:481 hour, 25 minutes, 48 secondsdoing, especially when you pair it with tools like Excel or Google sheets or whatnot. Now I came up with this idea of directive orchestration executions.
1:25:571 hour, 25 minutes, 57 secondsIt was this framework that I put about probably about like four or five months ago, just as Claude was figuring out how skills worked and stuff like that.
1:26:051 hour, 26 minutes, 5 secondsAnd they've since created skills, which I think is actually a much better alternative to my DOE framework.
1:26:101 hour, 26 minutes, 10 secondsSo I just use skills now, but basically what these things are, are just like subagents. These are high level instructions that you can give to the parent agent. Okay.
1:26:201 hour, 26 minutes, 20 secondsThe one distinction between subagent and skills is in the subagent, it does it all look like a different agent in the skill.
1:26:251 hour, 26 minutes, 25 secondsIt's like given to the parent agent and basically it's just a list of instructions allows it to do something.
1:26:291 hour, 26 minutes, 29 secondsSo I want to give you guys a brief little example of what that might actually look like using a skill that I developed called shop Amazon.
1:26:341 hour, 26 minutes, 34 secondsSo heading back to our folder here, if I go down to skills, you see that there is now a skill called shop-amazon.md. Up at the top right hand corner, the name is shop Amazon.
1:26:431 hour, 26 minutes, 43 secondsUnderneath here is browse and purchase items on amazon.ca via the Chrome dev tools MCP using the user as defined compare by products in Amazon.
1:26:501 hour, 26 minutes, 50 secondsThen there are a bunch of high level instructions about how exactly to use a various, like some various tools to browse Amazon for me and then find products
1:27:011 hour, 27 minutes, 1 secondthat I want to do, including stop, like get purchase approval, do not skip the step. So, I mean, like I often buy products on Amazon.
1:27:081 hour, 27 minutes, 8 secondsAnd to be honest, there's just so much junk on Amazon now that I don't want to have to spend every day hours of my time like rifling through mostly, you know, like SEO optimized garbage, which doesn't actually mean anything.
1:27:201 hour, 27 minutes, 20 secondsSo what I did is I put together a skill to do that for me. And at the moment, I require something to connect my basically in photography, like a bounce sheet or a reflector with one of my stands.
1:27:331 hour, 27 minutes, 33 secondsSo what I want to do is just, I'm going to speak into it. I'm going to say, Hey, I'm shopping for something to connect one of my reflectors to one of my tripod mounts.
1:27:441 hour, 27 minutes, 44 secondsI purchased the reflector a couple of days ago and I didn't realize that I needed, you know, something separate to kind of clip the two together. Could you shop Amazon and give me some options that I could use?
1:27:561 hour, 27 minutes, 56 secondsSo I'm just going to press enter here and then I'm just going to let it go on its way. We open up the thinking.
1:28:011 hour, 28 minutes, 1 secondWhat it's going to start with is the user is asking me to help them shop on Amazon. Then the user wants to find a reflector holder or whatever.
1:28:081 hour, 28 minutes, 8 secondsAnd now what it's going to do is actually an open up a Chrome tab for me using Chrome dev tools. And it's going to go and it's going to look for it. Okay. Now it knows that I'm in Canada for instance.
1:28:161 hour, 28 minutes, 16 secondsSo it's actually looking it up at Amazon.ca up here, scrolling through, it's going to open things up, take screenshots of various parts of the page.
1:28:251 hour, 28 minutes, 25 secondsIt's going to read through everything and so on and so forth.
1:28:271 hour, 28 minutes, 27 secondsAnd it'll actually at the end of it, get me a bunch of options, according to what I wrote in my, you know, shop dash Amazon markdown skill.
1:28:351 hour, 28 minutes, 35 secondsAnd so if you think about it, like this is something that previously a virtual assistant or something might have done, right? I mean, this is something that like I would have just given to
1:28:431 hour, 28 minutes, 43 secondssomebody and delegate it away. Hey, you know, I'm setting up a photography studio in X, Y, and Z. Well, now I can actually just write a skill, a high level skill that teaches it how to use Amazon.
1:28:521 hour, 28 minutes, 52 secondsAnd then once it goes through Amazon and you know, finds me the products, then just gives me like a big list of things like this. So what I can do now is I could say, Hey, you know, I want to buy X, Y, and Z.
1:29:011 hour, 29 minutes, 1 secondAnd then I can go and actually buy it for me. You know, obviously I recommend, if you guys are like making purchase decisions with Claude Code, this isn't really something I'd a hundred percent automate. You know,
1:29:101 hour, 29 minutes, 10 secondsmaybe I'd have it add all the products to cart. And then I'd say, Hey, give me the page so I can review it and then purchase it myself. But you can automate this about as crazily in detailed as you want.
1:29:191 hour, 29 minutes, 19 secondsWhat we've done is we basically made an API out of Amazon and they don't have one specifically because they don't want everybody to. With cloud skills, you can do something like that super easily.
1:29:281 hour, 29 minutes, 28 secondsThe variety of other skills that you can create, this one's called Upwork Scrape Apply. I have a bunch that do like, you know, lead scraping for me more generally.
1:29:361 hour, 29 minutes, 36 secondsI have skills that automate the process of sending welcome emails to new clients. I have skills that automate the process of building their deliverables.
1:29:421 hour, 29 minutes, 42 secondsAnd what's really cool is you're not the only person that had, like you don't actually have to put the whole skill together yourself.
1:29:481 hour, 29 minutes, 48 secondsYou can just have cloud help you put the skill together for a future instance of cloud. And in practice, that's usually what I do. I'll say something like, Hey, I want to build a skill that does X, Y, and Z. Can you help me format it?
1:29:571 hour, 29 minutes, 57 secondsHere's like how skills work. Cause sometimes it won't know for whatever reason.
1:30:001 hour, 30 minutesIt'll have to go research skill formatting and stuff like that. And then we'll say, yeah, sure. I could put one together for you.
1:30:051 hour, 30 minutes, 5 secondsThen what you do is you take that feed that to a fresh instance of Claude Code that has no understanding of what the skill is, see how it does.
1:30:101 hour, 30 minutes, 10 secondsIf it screws up, you just give it feedback and say, can't modify the skills that you do better next time.
1:30:141 hour, 30 minutes, 14 secondsYou rinse and repeat and eventually you get an error rating, which may start off at like, I don't know, let's say like it, it's only good 70% of the time on your first. Well, after some changes,
1:30:241 hour, 30 minutes, 24 secondsnow it's good 80% of the time. Then after a couple more changes, now it's good 90% of the time. And then eventually I want to say, you can get to like 98 to 99% fidelity and accuracy, which in any sort of knowledge field nowadays is more than enough.
1:30:361 hour, 30 minutes, 36 secondsI'd say most human beings screw up more than one to 2% of the time.
1:30:391 hour, 30 minutes, 39 secondsSo we'll cover a little bit more about skills and how to create them, how to take preexisting SOPs and workflows and stuff like that, and convert them into skills a little bit later on. But for now, just know that they're there. Okay.
1:30:491 hour, 30 minutes, 49 secondsSo most everything the year has now been covered. We talked about Claude, we've talked about the Claude.md, we've talked about the local, we talked about the agents folder, the skills folder, the rules folder.
1:31:001 hour, 31 minutesWe only have a few things left. Like there's MCPs to talk about, but now's not a good time to, so I'm going to push that off to later.
1:31:061 hour, 31 minutes, 6 secondsAnd then also the settings.json is a good thing to mention, but since this deals with hooks, I'll also talk about that later.
1:31:111 hour, 31 minutes, 11 secondsYou're now at the point where you understand, I want to say, you know, 90% of the internal workings of Claude code.
1:31:171 hour, 31 minutes, 17 secondsYou understand the file structure, the organization, you understand the highest ROI way to build anything, whether it is a simple website or something more complex,
1:31:251 hour, 31 minutes, 25 secondslike a full stack app or an automation. From here on out, it's really just learning a little bit more about Claude's various modes.
1:31:301 hour, 31 minutes, 30 secondsSo plan mode, dangerously skip permissions, you know, ask before editing and so on and so forth.
1:31:361 hour, 31 minutes, 36 secondsAnd then we can take a listen and we can use it to build something really, really cool.
1:31:391 hour, 31 minutes, 39 secondsWhat we're going to learn about next are the various permission modes available to us in Claude code. Now, just so we're all on the same page here, when I say permission mode, what I'm referring to is this little button down at the very bottom of the GUI.
1:31:531 hour, 31 minutes, 53 secondsAnd you can toggle through this button pretty straightforwardly and easily. And as you can see here, when we do, we get four main modes.
1:32:001 hour, 32 minutesThe first is ask before edits. The second is edit automatically. The third is plan mode. And the fourth is bypass permissions.
1:32:081 hour, 32 minutes, 8 secondsI should note that you're not actually getting a bypass permissions right out of the gate here, at least not as of the time of this recording. So I'll show you guys how to enable that yourselves.
1:32:161 hour, 32 minutes, 16 secondsSo we're going to run through each of these as well as some extras.
1:32:181 hour, 32 minutes, 18 secondsAnd then at the end of today's module, we're going to focus significantly more on plan mode.
1:32:221 hour, 32 minutes, 22 secondsI'm going to walk you guys through how plan mode works, why you might want to use it, and then ultimately how to use plan mode to build something that I've personally been wanting to build for quite a while. So we're going to do it interactively together.
1:32:321 hour, 32 minutes, 32 secondsSo permission modes control how your agents handle permissions.
1:32:371 hour, 32 minutes, 37 secondsYou also give the current permission mode to any sub agents that you employ, which is going to be pretty important for later.
1:32:441 hour, 32 minutes, 44 secondsNow they tend to inherit the permission context from the main conversation, but there are a couple of situations in which they can actually override the mode too. For now,
1:32:521 hour, 32 minutes, 52 secondsI just want you to pretend that all we're talking about are our current top level agents. We're not focused on any sub agents or any additional functionality, nothing like what we just talked about earlier.
1:33:031 hour, 33 minutes, 3 secondsSo we have default. Default is standard permission checking with prompts.
1:33:081 hour, 33 minutes, 8 secondsYou guys remember down here where it says ask before edits, you guys can think of this as basically the default. Okay.
1:33:141 hour, 33 minutes, 14 secondsAnd so the default setting is before Claude makes any changes to any files on your computer, it has to ask you whether or not it's okay to do it.
1:33:241 hour, 33 minutes, 24 secondsAnd I'll show you guys what that looks like right now by saying, you know, change the title of the project to Nick's happy fun time.
1:33:371 hour, 33 minutes, 37 secondsSo because I'm in ask before edits mode, you'll see that before it does any sort of change, what it's going to do is it's first going to look at the specific file that
1:33:451 hour, 33 minutes, 45 secondsdefines the title. It's going to pop open on the right hand side, the exact section of the page that it's considering updating.
1:33:531 hour, 33 minutes, 53 secondsSo initially it's a profile name dash work log profile name.
1:33:551 hour, 33 minutes, 55 secondsIn this case was Nick. It defines some really cool, badass variable stuff for me, but because of my dumb request at now saying title equals Nick's happy fun time
1:34:021 hour, 34 minutes, 2 secondsin lowercase, you'll also notice, I'm just going to have to remove my head here so that we can see this a little bit better. You'll also notice that down at the bottom, it says, Hey, should we make this edit to index.astro? That's the file.
1:34:141 hour, 34 minutes, 14 secondsAnd I have three choices. I can either say yes, by clicking or pressing one, two saying yes, allow all edits this session or three. I could say no.
1:34:241 hour, 34 minutes, 24 secondsAnd finally, I can also say tell Claude what to do instead. JK please don't do this. And so because of this, it's going to say no changes made.
1:34:331 hour, 34 minutes, 33 secondsAnd I will not have actually gone through the request. Obviously, most of the time we don't actually do that. We don't actually make that third,
1:34:401 hour, 34 minutes, 40 secondsor rather we don't click that fourth one. As you see, it's also kind of annoying, but generally speaking, if you guys are working in a code base, that is,
1:34:481 hour, 34 minutes, 48 secondsI don't know, really high risk, sort of high reward thing, where like every change needs to be good, or it's going to screw everything up. You can use ask before edits.
1:34:561 hour, 34 minutes, 56 secondsI should note that very few people are nowadays. We moved away from ask before edits.
1:35:011 hour, 35 minutes, 1 secondMost people now use either the next setting them to show you, or they just bypass permissions like me entirely. The next major setting is accept edits and accept edits.
1:35:101 hour, 35 minutes, 10 secondsWhat we do is we auto accept any edits to files, but then if you want to create new files, it'll still ask you for it.
1:35:161 hour, 35 minutes, 16 secondsAnd so going back to our little Claude code page here, we moved from ask before edits to edit automatically. Okay. We can now edit any preexisting files.
1:35:261 hour, 35 minutes, 26 secondsSo we can do is we could say, sorry, I actually want you to do this, update the project to the title.
1:35:351 hour, 35 minutes, 35 secondsAnd because we've selected edit automatically instead of ask before edits, it'll actually go through and it'll automatically update that for me. See how there was no little panel on the right hand side.
1:35:441 hour, 35 minutes, 44 secondsSo this is useful when you want to give the model like carte blanche control over any preexisting files, but you don't want it to like have any control or any ability to make new ones.
1:35:531 hour, 35 minutes, 53 secondsSo I'm just going to say revert the change and keep in mind that now, because we're in edit automatically, it can do so without actually having to pop.
1:36:001 hour, 36 minutesThe next one is don't ask. Now there's no, don't ask permission prompt explicitly set up here.
1:36:061 hour, 36 minutes, 6 secondsSo if you want to get to your permissions, you actually have to go backslash permissions and then continue in a terminal. This is going to open up a new page for you.
1:36:131 hour, 36 minutes, 13 secondsThat's going to then pump in Claude with some permissions tab.
1:36:171 hour, 36 minutes, 17 secondsAnd then you're going to get a list of all of the different permissions that you can have, including rules in this workspace. So as you can see, we have allow, ask, deny workspace. Okay.
1:36:291 hour, 36 minutes, 29 secondsSo this is equivalent to our edit all tab. Deny will always reject requests to use any tools.
1:36:361 hour, 36 minutes, 36 secondsAsk will always ask for confirmation before using tools and allow won't ask before using any. What's cool is you also have the ability to add a new rule.
1:36:441 hour, 36 minutes, 44 secondsSo permission rules are basically where you give it a, the name of a tool and then you either allow it to use the tool or you force it
1:36:511 hour, 36 minutes, 51 secondsto ask you for permissions before using a tool that obviously takes us to that logical question. Well, what are tools, Nick? We haven't talked about them. Well, the variety of different ones that Claude code could use.
1:37:001 hour, 37 minutesThere's stuff like the ability to fetch things from the web.
1:37:031 hour, 37 minutes, 3 secondsThere's stuff like bash, which is the ability to write like terminal commands and whatnot. And, you know, the purpose of this course is not to go through every single one of the tools.
1:37:091 hour, 37 minutes, 9 secondsCause to be honest, they're always changing the tools and like the sorts of tools that we have and that's not super valuable, but it's just so that, you know,
1:37:161 hour, 37 minutes, 16 secondsyou can identify and then change on a like file or tool basis, which things Claude code has access to. So long as you're hyper, hyper specific about it using in this case, you know, this little tools output.
1:37:301 hour, 37 minutes, 30 secondsThe next tab is delegate.
1:37:321 hour, 37 minutes, 32 secondsNow this is a coordination mode for agent team leads. Basically, the Claude code now has that feature called the agent teams feature where a single agent up at the top can delegate a bunch of work to a bunch of sub agents.
1:37:431 hour, 37 minutes, 43 secondsAnd so this is the permission that the agent team lead is given, which basically allows them to delegate tasks. Although, it's not allowed to do anything aside from just team management tools.
1:37:521 hour, 37 minutes, 52 secondsWe'll talk a little bit more about that later. Then we have bypass permissions. This is what I've been using up until now in basically all instances.
1:38:001 hour, 38 minutesBypass permissions is great because you can do whatever the heck you want. I should note that there is obviously a risk here.
1:38:051 hour, 38 minutes, 5 secondsThere was a case a little while ago where somebody had Claude code running on bypass permissions. And then I think it was on like a Linux computer or something,
1:38:121 hour, 38 minutes, 12 secondswhere there's a simple terminal command that you could use to basically delete everything on your computer. It's like sudo rm-rl or rf or something like that.
1:38:191 hour, 38 minutes, 19 secondsI don't remember the exact command. I'm sure Claude would be able to tell you.
1:38:231 hour, 38 minutes, 23 secondsAnd basically because of a misinterpretation of the request and, you know, it did a bunch of research on its own, whatever, it eventually thought it had to run this command.
1:38:301 hour, 38 minutes, 30 secondsSo it ran the command and it basically deleted all of the data on the person's hard drive. They basically had it bricked and then they needed to take it in to fix it. I want you to know that these sorts of things are possible, of course,
1:38:401 hour, 38 minutes, 40 secondsand I'm not a lawyer, so don't sue the hell out of me if the sense of happening to you, but it's very unlikely in practice. This sort of thing occurs, it's a vanishingly small percentage of the time.
1:38:491 hour, 38 minutes, 49 secondsAnd nowadays with agents getting more and more autonomy and other things, and then more and more skill and more ability to plan their own work, like we're going to talk about in a moment with plan mode. You know, most people are shifting towards using bypass permissions.
1:39:011 hour, 39 minutes, 1 secondBypass permissions also allows Claude to create new files, not just delete them.
1:39:051 hour, 39 minutes, 5 secondsThat in addition to editing files can present a risk. The main risk, if we're just being like business minded here, is actually you just like,
1:39:111 hour, 39 minutes, 11 secondsyou create a bunch of additional files that maybe you don't need. And, you know, because of that, your workspace can bloat over time.
1:39:181 hour, 39 minutes, 18 secondsSo it's pragmatic and pertinent to every now and then just ask Claude code to go through your files and see if there's anything in the workspace that just isn't required anymore. You know, realistically,
1:39:261 hour, 39 minutes, 26 secondsas you guys are going to see when we build this next project, Claude's going to try a bunch of approaches to do things, both on the front end and the back end, although the back end,
1:39:341 hour, 39 minutes, 34 secondsusually much more often. And in doing so, it'll accumulate like different libraries that it probably doesn't need. It'll accumulate different files.
1:39:411 hour, 39 minutes, 41 secondsIt'll create temporary Jasons and all this fancy stuff. And as a result of that, if you're not constantly on top of that, you can have a folder that has like 10,000 files and it's all just temp stuff, which slows down your computer and bloats Claude code. I've done it before.
1:39:551 hour, 39 minutes, 55 secondsSo we'll talk a little bit more about context management, how to effectively do that in one of the next modules.
1:39:591 hour, 39 minutes, 59 secondsBut I just wanted you guys to know that for now in terms of how to set up bypass permissions, it's actually non-trivial to do this.
1:40:051 hour, 40 minutes, 5 secondsAnd if it's the very first time that you're setting up Claude code, you won't have access to that. So head over to the extensions tab, get into Claude code for VS code. You're going to want to click this little gear icon and go to settings.
1:40:151 hour, 40 minutes, 15 secondsThat's going to open up this tab over here. I'm just going to move it over to the middle so we could see.
1:40:191 hour, 40 minutes, 19 secondsYou'll notice that one of the first settings is Claude code allowed dangerously skip permissions. So it'll recommend this only for sandboxes with no internet access.
1:40:271 hour, 40 minutes, 27 secondsObviously mine has internet access just fine. So, you know, accept this at your own risk, but if you click this button, you will now have access to it down below.
1:40:341 hour, 40 minutes, 34 secondsThere's a few other settings here like Claude code auto-save enable new conversation shortcuts, disable login prompts, and so on and so forth. I don't really use or change any of these in practice. Okay.
1:40:431 hour, 40 minutes, 43 secondsAnd then finally you have plan mode, which is going to make up the bulk of what we're talking about next plan mode is read only exploration, which basically means Claude code can research things using web tools.
1:40:561 hour, 40 minutes, 56 secondsSo you can go on the internet and find things out for you. It can read through all the preexisting files in your directory.
1:41:011 hour, 41 minutes, 1 secondIt can also reason from first principles and it can kind of use its own intelligence to figure things out.
1:41:051 hour, 41 minutes, 5 secondsAnd then it can basically take all of this and put this into a plan document before presenting it to you. Now plan mode is awesome.
1:41:121 hour, 41 minutes, 12 secondsAnd I use plan mode all the time and basically anytime I'm doing any sort of build that's more complicated than a simple design.
1:41:171 hour, 41 minutes, 17 secondsThe reason why it's so good is because instead of acting, which in the real world takes a lot of time and energy to both do and then undo
1:41:261 hour, 41 minutes, 26 secondsall plan mode does is it just researches all the factors involved in the build before doing it.
1:41:301 hour, 41 minutes, 30 secondsIf you work in this like theoretical plan space and not the actual like space of the, you know, the build and all the libraries and all the code,
1:41:391 hour, 41 minutes, 39 secondsyou will save many, many hours of building over the course of just the next few days and probably tens and hundreds of hours over the course of a lifetime of using this tool.
1:41:491 hour, 41 minutes, 49 secondsA minute of planning saves you 10 minutes of building. It's just super high leverage and I'd recommend you.
1:41:531 hour, 41 minutes, 53 secondsSo imagine two possible scenarios for me in the first scenario, you build something with Claude code, then you test it and then you realize that there's some issue with it.
1:42:031 hour, 42 minutes, 3 secondsMaybe you're building a simple web app that, you know, upon login adds some numbers or credentials to a database. So you've done this now you've realized that it's wrong.
1:42:131 hour, 42 minutes, 13 secondsWhat that means is because the approach is wrong.
1:42:151 hour, 42 minutes, 15 secondsBasically the time that you spent building while not completely wasted, a big chunk of it is wasted. Okay.
1:42:211 hour, 42 minutes, 21 secondsSo not only have you spent the 15 minutes to build the thing, not only you spent the five minutes to test the thing, you also have to rebuild the thing, which can take 15 minutes multiplied by however many times you have to continuously test and retest.
1:42:311 hour, 42 minutes, 31 secondsThat means that the total amount of time it takes you is 35 minutes plus a fair number of tokens. Not a lot of people talk about, but this can obviously eat into costs. That is scenario one.
1:42:401 hour, 42 minutes, 40 secondsAnd this is the build without plan approach.
1:42:451 hour, 42 minutes, 45 secondsOkay. Now in scenario two, which is the build with plan, what you do is you spend your first five minutes, just planning something super in depth with Claude codes, plan mode,
1:42:551 hour, 42 minutes, 55 secondssomewhere during the plan, because we're folk, we're, we're building a super like a granular line item scope here.
1:43:011 hour, 43 minutes, 1 secondWe're looking at all the tools and we're looking at the objects and whatever the heck, there's like a lot that's going on out of the hood because we're doing that. Claude code realizes that it won't work halfway through, and then just recreates a better plan that does it.
1:43:121 hour, 43 minutes, 12 secondsThe total amount of time it takes for you to like get to the building is just five minutes plus five minutes, 10 minutes.
1:43:181 hour, 43 minutes, 18 secondsAnd then maybe your actual build time now, because it's like so much better and faster and stuff like that is only five minutes or 15. So you think about it,
1:43:251 hour, 43 minutes, 25 secondslike not only have we saved 20 minutes on a single build, and we've also done so with significantly fewer tokens.
1:43:311 hour, 43 minutes, 31 secondsWhat that means is it's much better to like do all of your work here, basically during the planning of the spec.
1:43:411 hour, 43 minutes, 41 secondsAnd this is true not only from Claude code, but any sort of programming or really any sort of project development, as opposed to here, which is like where, you know,
1:43:511 hour, 43 minutes, 51 secondsyour machines are actually building this thing, like this fantastic, amazing Lego block based construction.
1:43:571 hour, 43 minutes, 57 secondsI'm just going to pretend that like we're building some sort of building or pyramid here, right? Because, you know, if you screw this up,
1:44:041 hour, 44 minutes, 4 secondswhat that means is now you have to knock all these Lego blocks down, and then you have to rebuild it from scratch all over again.
1:44:091 hour, 44 minutes, 9 secondsSo better to go off the blueprint or the architecture diagram or whatever, and make changes there than in the physical world.
1:44:151 hour, 44 minutes, 15 secondsThe physical world incurs a fair amount of real costs. By the way, I know we're working in the virtual world here, but it's the same thing as like planning a construction project, right?
1:44:221 hour, 44 minutes, 22 secondsYou plan construction projects that you don't run into a situation where you don't have enough materials on site. And you're like, Oh my God, I got to fricking stop everything for the day and go find some.
1:44:301 hour, 44 minutes, 30 secondsSo how do you actually use plan mode in reality? Well, what I want to do next is I want to use plan mode to build out a pretty complicated project.
1:44:371 hour, 44 minutes, 37 secondsThis project is going to basically be a full stack web application. It's going to have a front end. It's going to have authentication and like an interface where you can log in.
1:44:461 hour, 44 minutes, 46 secondsAnd it's also going to have a backend and we're going to build it in just a few minutes.
1:44:491 hour, 44 minutes, 49 secondsThe specific project that I'd like to build today is basically a proposal generation platform.
1:44:551 hour, 44 minutes, 55 secondsI want to automatically be able to generate proposals, high quality sales documents that I can then send to prospects through this web interface. I want to do it all natively.
1:45:051 hour, 45 minutes, 5 secondsAnd I basically want to rebuild the functionality of, I don't know, like docuSign or like Panda doc. I want there to be all the bells and whistles on it.
1:45:131 hour, 45 minutes, 13 secondsI want there to be like the ability for people to sign, but also to like pay. I want to have my own little login screen so that I can give it to my clients.
1:45:211 hour, 45 minutes, 21 secondsAnd then maybe my colleagues. And I can obviously also use it myself.
1:45:241 hour, 45 minutes, 24 secondsI want to, you know, have like a couple of templates that I produce based off of it basically end to end. I want to build a fricking app today. This is much more complicated than just a simple landing page. Right?
1:45:331 hour, 45 minutes, 33 secondsSo how am I going to go about doing it? Well, the first thing I'm going to do is I'm actually just going to build out what I'd consider to be a pretty straightforward project spec, which is just a list of things that I want this to be able to do.
1:45:431 hour, 45 minutes, 43 secondsAnd there's a bunch of different formatting methodologies here and like different ways of doing it. You don't really have to worry too much about that.
1:45:491 hour, 45 minutes, 49 secondsAll I'm going to do is I'm basically going to dump everything in via voice transcript to a little text tab.
1:45:541 hour, 45 minutes, 54 secondsAnd then I'm going to feed that into Claude Code and have it actually format that into a specs document for me. So I'm going to open up my voice transcription tool and get after it.
1:46:021 hour, 46 minutes, 2 secondsMy goal today is to build a proposal generation platform.
1:46:061 hour, 46 minutes, 6 secondsI want this proposal generation platform to have everything that a common tool like Panda doc might have in so far that I want it to be able to generate end to end high quality proposals as okay.
1:46:201 hour, 46 minutes, 20 secondsSo I just did that. I have a tremendous amount of context.
1:46:231 hour, 46 minutes, 23 secondsNow what I'm going to do is I'm actually going to go to a new window and antigravity, which just closed out of the old one. I'm then going to open up a new folder. So go open folder.
1:46:321 hour, 46 minutes, 32 secondsAnd here I'm going to say new one. Let's just call this proposal generator app. Once I've created this, I'm going to dump right in.
1:46:401 hour, 46 minutes, 40 secondsThen I'm going to go to Claude Code here. Let me zoom in so we could see this a little bit better down here. I'm going to go, um, sorry, not by a past permissions plan mode.
1:46:491 hour, 46 minutes, 49 secondsAs you can see, I'm pretty eager. And then I'm going to go back here, copy this and then just dump all this in. It's for a matter of white space.
1:46:561 hour, 46 minutes, 56 secondsSo bear with me. And what I did here is I just, I just dumped in more or less everything that I wanted to do in the app. So I didn't specify things in a technical way.
1:47:041 hour, 47 minutes, 4 secondsI just told it what I wanted. My goal today is to build a proposal generation platform.
1:47:081 hour, 47 minutes, 8 secondsI want this proposal generation platform to have almost everything that a common tool like Panda doc might have, except for the template builder functionalities. I just want to give you a template and have you do it.
1:47:161 hour, 47 minutes, 16 secondsAside from that, I want to be able to generate end to end high quality proposals as static pages that I could send the URL to the client with. And now it's going to ask me a bunch of questions about it.
1:47:241 hour, 47 minutes, 24 secondsSo what front end framework do you want to use? I don't know, whatever's the best.
1:47:281 hour, 47 minutes, 28 secondsSo I'm just going to say this one. Sure. For e-signatures, how legally robust do you need them to be? Um, I don't know what that means. I'll just click the simplest one for straight payments.
1:47:351 hour, 47 minutes, 35 secondsWill proposals have a fixed price or variable amounts you set on proposal? That's a great question. I'll say variable are using super base for the database too. I'll say super base for everything. Cool. Submit answers.
1:47:451 hour, 47 minutes, 45 secondsSo what basically this just did is it crafted a little graphically user interface for me to ask me some questions about specific ways that it wants to do the project. Um, and in this way we can go back and forth, which is quite nice.
1:47:561 hour, 47 minutes, 56 secondsOkay. Tailwind for utility, CSS, shad, CN UI for polished. I don't know what the hell that means. Let's just click it. Can you share the proposal template now? Paste it, link it, or tell me the thought path.
1:48:051 hour, 48 minutes, 5 secondsI'll paste it next message. That sounds great.
1:48:081 hour, 48 minutes, 8 secondsSo what I'm going to do now is I'm going to go find a template of a proposal that I want it to automatically generate for me. Okay.
1:48:131 hour, 48 minutes, 13 secondsSo I have my proposal template over here. It's pretty sexy. You know, I give people some problem areas, some solutions. Um, you know, I talk about why us, I have a little photo of me, Alex Ramosi and Sam Evans up there.
1:48:231 hour, 48 minutes, 23 secondsSo it's pretty sexy. What I'm going to do next. I'm just going to move this into my workspace one-time project. And over here, I'm just going to rename this to call this proposal template.
1:48:331 hour, 48 minutes, 33 secondsThat's okay. And then over here, I'll say, great, it's in proposal template.pdf.
1:48:391 hour, 48 minutes, 39 secondsAnd, um, just because I also want the design to be really cool, use a simple clean design, sort of like, uh, Apple,
1:48:461 hour, 48 minutes, 46 secondsfollow the proposal template design in the actual generation of the page for everything else though, make it kind of Apple-esque. Okay. Next up, it'll read through my proposal template and then think up what to do next.
1:48:571 hour, 48 minutes, 57 secondsAnd now it is generating a plan for me. It's figured out the nine page proposal document.
1:49:011 hour, 49 minutes, 1 secondIt's designing some detailed implementation thing with all the information, the user flow and so on and so forth. What's interesting is it's giving this to a sub agent.
1:49:091 hour, 49 minutes, 9 secondsYou could see because it's using the task feature, which is basically coded sub agent language. As you can see, there's a tremendous amount of information that it's going through in order to
1:49:171 hour, 49 minutes, 17 secondsgenerate this. It's also doing some research, like looking up things from Panda doc, just because I referenced it. Okay. And at the end it's now finished the final plan file.
1:49:251 hour, 49 minutes, 25 secondsSo what I'm going to do is I'm just going to scroll through it and read it for myself. It's very comprehensive proposal generator platform implementation plan.
1:49:311 hour, 49 minutes, 31 secondsWe're going to build a Panda doc like proposal generation platform for left like users will sign in, create proposals via AI and share public URLs with clients. Clients will view sign canvas signature and pay.
1:49:421 hour, 49 minutes, 42 secondsThe proposal page will follow the provided PF template design. Also the app is Apple esque and minimal. Here's the tech stack.
1:49:481 hour, 49 minutes, 48 secondsI don't know what most of that stuff means to be honest, and I'm not going to worry about it. Proposal template sections, cover your problem areas, your solution. Why us, our team,
1:49:561 hour, 49 minutes, 56 secondswhat working with us looks like, what your investing contracts, signature payment, database schema profiles. I don't know again, what the heck this means. I'm not going to worry about it.
1:50:041 hour, 50 minutes, 4 secondsAnd then over here we have a bunch of routes, API things, false structures, you know, as somebody that is not a developer by trade,
1:50:101 hour, 50 minutes, 10 secondsI'm not going to focus too much on that stuff, but it looks like when people sign in, they hit log in and those will be a dashboard page. When they create, they'll click new proposal, which will go to dashboard slash new.
1:50:201 hour, 50 minutes, 20 secondsThere'll be a few form fields to fill out like brief description and pricing rows. They'll submit it. That'll call Opus and then we'll generate them. And then in order to copy, we just copy this URL and send it to the client.
1:50:301 hour, 50 minutes, 30 secondsThat looks pretty clean to me. I'm sure it's not going to be perfect, but yeah, why don't we give it a go? So what I'm going to do is I'm going to so auto accept.
1:50:371 hour, 50 minutes, 37 secondsAnd I know just because I've done some things before with this tool stack, SuperBase specifically, I'm just going to go through it.
1:50:441 hour, 50 minutes, 44 secondsI'm going to set up a SuperBase account while it's running me through all of the stuff that way I can kind of, you know, double up on the time. While this does some work for me, I can go and do the, the SuperBase stuff.
1:50:521 hour, 50 minutes, 52 secondsSo SuperBase is a simple database.
1:50:531 hour, 50 minutes, 53 secondsBasically it just handles like the logins and also handles like the generation of records and stuff. First thing that you would want to do, if you were doing something similar, is you just log right into SuperBase.
1:51:011 hour, 51 minutes, 1 secondSet up a new account if you don't already have one and then start your project. I'm doing this for free.
1:51:051 hour, 51 minutes, 5 secondsSo I just started one called proposal underscore generator, and then I'll click on it, which will take me to the project. Somewhere on the left-hand side here, we have API keys.
1:51:131 hour, 51 minutes, 13 secondsAPI keys are basically just what we want to give to this so that it just does everything for me. So let's see here. We want to give it all keys.
1:51:231 hour, 51 minutes, 23 secondsSo I'm just going to go copy API key.
1:51:261 hour, 51 minutes, 26 secondsAnd then also I'm just going to, looks like it's asking me some questions here because it's now, oh, it's still in plan mode. So keep in mind, we want to go to bypass permissions mode now,
1:51:351 hour, 51 minutes, 35 secondsbecause instead of having to ask me every five seconds for things, you know, I want this to be able to proceed. And then I'm just going to give it some stuff. I'll say SuperBase, I don't know, secret key.
1:51:451 hour, 51 minutes, 45 secondsJust going to give it to it and also give it my SuperBase public key. Why am I doing all this?
1:51:521 hour, 51 minutes, 52 secondsBecause I know it's going to need this information in order to move forward.
1:51:551 hour, 51 minutes, 55 secondsNow in Stripe, I'm going to go over to one of my accounts and then I'll go test mode, create sandbox.
1:52:011 hour, 52 minutes, 1 secondWhat this will do is this will give me like a little sandbox version of Stripe that I could use with its own API keys and everything like that.
1:52:081 hour, 52 minutes, 8 secondsThis way I can basically like, you know, process the payments and stuff like that using this test. So here it is right now. And then if I want to get my API keys, I have them both over here.
1:52:171 hour, 52 minutes, 17 secondsSo I'm just going to copy the Publishal key. You know, I said, I want you to take payments during using Stripe basically, which is why it's doing this. Let's go public.
1:52:281 hour, 52 minutes, 28 secondsAnd then over here, I'll go private key. Cool.
1:52:321 hour, 52 minutes, 32 secondsAnd so now I basically loaded it up with what I think is everything that it'll need in order to actually go and like, you know, connect. So I'm just going to press enter here.
1:52:411 hour, 52 minutes, 41 secondsIn case you guys didn't know, when you press enter, what you do is you basically queue up another message.
1:52:451 hour, 52 minutes, 45 secondsSo when this is done with all of its tasks, it'll now have access to all of my keys and stuff.
1:52:491 hour, 52 minutes, 49 secondsSo now that it's done that task, it's going to create all the files and it's just adding all of the information and stuff like that. It looks like we have the SuperBaseAnon key.
1:52:571 hour, 52 minutes, 57 secondsI think that might be something else that we need. So I'm going to have to find that information out. It'll ask me to do this in a moment. So it's not that big of a deal. This is here.
1:53:041 hour, 53 minutes, 4 secondsIt just got my API keys. So it's going to update the ENV file.
1:53:071 hour, 53 minutes, 7 secondsAnd then at the end of this, it's probably just going to ask me like, Hey, can you also include X, Y, and Z? Now I could have, of course, just asked this thing to start building for me. You know, I could have just given it all the specs and said, go for it.
1:53:151 hour, 53 minutes, 15 secondsBut the planning that I did not only improves the probability that it'll be able to do this on a quote unquote one shot,
1:53:231 hour, 53 minutes, 23 secondsbut it also improves the token efficiency because it's not going to be exploring 10 different approaches at the time of building instead. You know,
1:53:311 hour, 53 minutes, 31 secondsit has like a document it can refer to. And that's kind of interesting, but human beings sort of do better that way too, right?
1:53:381 hour, 53 minutes, 38 secondsLike if they're in a business and then you give them an SOP, standard operating procedure, or you give them a checklist or something, or you give them a simple three-step rule, they always have to accommodate.
1:53:471 hour, 53 minutes, 47 secondsThey're much, much more likely to actually use those rules. So, uh,
1:54:021 hour, 54 minutes, 2 secondsyeah,
1:54:221 hour, 54 minutes, 22 secondsyeah, the time that I'm back, this thing is either still working or it's just wrapping up its completion.
1:54:281 hour, 54 minutes, 28 secondsI think right now we're like six or seven minutes in, but what's really cool is you can parallelize your work.
1:54:341 hour, 54 minutes, 34 secondsSo obviously this is all about being productive, but there is also sort of like a time management component to this as well.
1:54:391 hour, 54 minutes, 39 secondsLike after you do a plan and we're building a real big full stack app here, this is not a trivial enterprise.
1:54:441 hour, 54 minutes, 44 secondsAfter we do that, like we're gonna have to wait a few minutes.
1:54:471 hour, 54 minutes, 47 secondsSo, you can just set this aside, the value that this thing is going to get just watching, having me just watch it is quite low.
1:54:541 hour, 54 minutes, 54 secondsYou can actually just set this aside, let it continue the building and then come back either when it's done or when you hear that little hook chime go off, which is personally what I use to make sure I'm always in the loop.
1:55:031 hour, 55 minutes, 3 secondsAnyway, I'm gonna go marinate the salmon. When I come back, this app should be done.
1:55:061 hour, 55 minutes, 6 secondsOkay, so three or four minutes later, I just got back and I see that it is now good to go. It's just asking me for a few things.
1:55:121 hour, 55 minutes, 12 secondsSuperBase project URL, which I'll find, my Anthropic API key, I need to run an SQL migration, give it a Stripe webhook, then ultimately deploy to Netlify.
1:55:211 hour, 55 minutes, 21 secondsWhat I'm gonna do is I'm gonna focus on testing all this stuff locally, and then I'm gonna give it access to all this information.
1:55:271 hour, 55 minutes, 27 secondsAnd then after I'm done, I'll do the pushing and the deploying and I'll run and go see what that looks like.
1:55:311 hour, 55 minutes, 31 secondsKeep in mind, you don't need to have any computer programming experience to do this.
1:55:341 hour, 55 minutes, 34 secondsI mean, I didn't really give it anything that was programming specific, I just gave it a bunch of needs.
1:55:381 hour, 55 minutes, 38 secondsAnd while of course it went through and did a bunch of things that were most definitely programming, I wasn't really a part of that, which is quite valuable.
1:55:441 hour, 55 minutes, 44 secondsSo I'm gonna go find this information, I saw your next public SuperBase URL and then my Anthropic API key.
1:55:501 hour, 55 minutes, 50 secondsOkay, so I see it says, reference using APIs and URLs, this project ID, so I imagine that's probably that.
1:55:561 hour, 55 minutes, 56 secondsI'll say project ID for SuperBase is here, and then throw up a key, I'll just sign into Cloud real quick and grab, okay, so then I'm gonna grab this
1:56:051 hour, 56 minutes, 5 secondsand then over here, I'm just gonna call it proposal generator app.
1:56:101 hour, 56 minutes, 10 secondsAnd so I'm gonna give me a key that I could use to copy and no, you can't steal this from me because I I will have deleted it right after this.
1:56:191 hour, 56 minutes, 19 secondsNice try folks.
1:56:211 hour, 56 minutes, 21 secondsYou'd be surprised at how many YouTubers don't, which is hilarious, like half the YouTube API keys that you see still work like six months later. Be careful fellow YouTubers, run the SQL migration is next.
1:56:301 hour, 56 minutes, 30 secondsSo paste the contents of this thing into your SuperBase SQL editor. So I guess I need to do that myself. So I'm just gonna grab this, copy all this and then what?
1:56:401 hour, 56 minutes, 40 secondsSuperBase SQL editor and execute it.
1:56:421 hour, 56 minutes, 42 secondsOkay, while I'm doing that, I'm just gonna give it this and then where do I get that? SuperBase SQL editor.
1:56:501 hour, 56 minutes, 50 secondsOkay, there's one right over here. That looks like it, no clue what the heck I'm doing. I'm gonna click run, success, no rows returned, awesome.
1:56:591 hour, 56 minutes, 59 secondsI think that's what's supposed to happen. Anyway, we'll see, it'll tell me if there are any issues.
1:57:031 hour, 57 minutes, 3 secondsStripe webhook, register this in the Stripe dashboard and put the whatever secret in ENV various.
1:57:091 hour, 57 minutes, 9 secondsI don't know what that means and I honestly don't think I need to do that. So I'm just gonna ask, okay, let's test this puppy locally. Okay, so it's giving me the information.
1:57:181 hour, 57 minutes, 18 secondsIt's also saying that the local host thing is ready to go.
1:57:211 hour, 57 minutes, 21 secondsSo I'm actually just gonna open this up, paste this in and see, cool, I got it. So it says I'm gonna have to confirm my email. So I don't really like that.
1:57:291 hour, 57 minutes, 29 secondsSo the first thing I'm gonna do is I'll say, looks good.
1:57:321 hour, 57 minutes, 32 secondsIf the user email isn't confirmed, don't give it to them in a red error message that's kind of unfriendly.
1:57:401 hour, 57 minutes, 40 secondsJust tell them to check their email after their initial signup because right now there's no notification with that.
1:57:471 hour, 57 minutes, 47 secondsAnd then basically I'm just gonna like work through this step by step, page by page.
1:57:511 hour, 57 minutes, 51 secondsOkay, and the first thing I'm getting is I check my email inbox. I'm not seeing an email.
1:57:551 hour, 57 minutes, 55 secondsSo I'm just going to give it a message telling it, hey, first of all, let them know that they need to confirm their email.
1:58:021 hour, 58 minutes, 2 secondsSecond of all, actually make sure that the email is being confirmed because I'm not getting it upon the sign in.
1:58:061 hour, 58 minutes, 6 secondsOkay, and then it gave me the ability to turn off the toggle email. So I'm just gonna save that. So we now no longer need to confirm the email.
1:58:151 hour, 58 minutes, 15 secondsAnd I'm gonna go back here. Okay, cool, and it looks like I'm now into the dashboard.
1:58:191 hour, 58 minutes, 19 secondsBottom left-hand corner, we have what looks to be, I don't know, some Next.js stuff, I think. I'm not really sure what this is. It might just be like some developer stuff.
1:58:261 hour, 58 minutes, 26 secondsOn the top right-hand corner, it looks like we can sign out. So let me just try signing out. Cool, and now in the middle, we can create a new proposal.
1:58:341 hour, 58 minutes, 34 secondsJust says proposals up here. So click create new. Now there's a bunch of information. I like this. So why don't I just go my own information?
1:58:421 hour, 58 minutes, 42 secondsI wonder if I just generate proposal if that's gonna work. Let's do 1,000, 1,500, 2,000. Okay, and then AI, Powered Sales Pipeline. I actually like this, so why don't we do that.
1:58:501 hour, 58 minutes, 50 secondsThe client needs an automated lead generation system that integrates with their existing CRM.
1:58:541 hour, 58 minutes, 54 secondsThey currently spend 20 hours a week on manual outreach and wanna reduce this to under five hours while increasing qualified leads by 3x. I know they want to get to 100K a month.
1:59:021 hour, 59 minutes, 2 secondsLet's do that. Okay, now for the money shot, let's generate proposal. Click on the button, don't know what's going on.
1:59:111 hour, 59 minutes, 11 secondsNo clue whether this is working.
1:59:121 hour, 59 minutes, 12 secondsGenerally speaking, when you see a little bar like this with a little circular thing, like this is pretty poor in terms of like user experience because I just don't know if it's working or not.
1:59:221 hour, 59 minutes, 22 secondsI'm not really sure.
1:59:231 hour, 59 minutes, 23 secondsBe nice if there could be some sort of progress, some way that I could see the thing actually being generated or upon clicking this, it'd be nice if I went to a new page.
1:59:311 hour, 59 minutes, 31 secondsSo I think I'm probably gonna do that. Hey, I'm not sure if the proposal has been generated. It's been 10 or 15 seconds right now.
1:59:391 hour, 59 minutes, 39 secondsCould we do some additional user feedback after they click the generate proposal button? Some sort of status, some sort of updates.
1:59:471 hour, 59 minutes, 47 secondsBasically, there just needs to be some way that I know that the proposal is actually being generated, not just hanging all day. Okay, it did end up generating the proposal after a while.
1:59:551 hour, 59 minutes, 55 secondsIt looks very clean, but still I want you to do this. Okay, so I'm just gonna feed that in here. I'm really liking this. I mean, look at the logo even, that's very sexy.
2:00:032 hours, 3 secondsUsing the same font, nice confidential. Oh, this is so sexy. Look at that, huh? Wow, I just built a proposal for this.
2:00:132 hours, 13 secondsWhat I'm gonna do now is just give it some more feedback.
2:00:152 hours, 15 secondsI don't like how the text immediately under your problem areas is really constrained width-wise.
2:00:242 hours, 24 secondsYou should make that a little longer, maybe two times as wide.
2:00:302 hours, 30 secondsIn each of the sub benefits underneath 01, 02, 03, 04, it's a little too wide now.
2:00:392 hours, 39 secondsSo make that maybe 75% as wide. Do the same thing with the text under your solution. Under YS looks great.
2:00:482 hours, 48 secondsI want to have that image of myself, Alex Formosy and Sam Ovens in there somewhere.
2:00:532 hours, 53 secondsSo find a way to include the image in a high quality manner.
2:00:582 hours, 58 secondsThere's some minor spacing problems with the we've done this before, we focus on money and we don't treat AI as a fad.
2:01:072 hours, 1 minute, 7 secondsThey're not perfectly lined up to the numbers one, two, three on the left-hand side. Add some images of myself and Noah.
2:01:172 hours, 1 minute, 17 secondsThe what you're investing looks pretty clean.
Chapter 14: Adjusting the Proposal Design
2:01:252 hours, 1 minute, 25 secondsBut in general, there's a bit of a discord between everything being left aligned and then the service agreement being in white in the middle.
2:01:352 hours, 1 minute, 35 secondsFind a way to fix that. Okay, and now there's one more thing I want to do.
2:01:432 hours, 1 minute, 43 secondsI just want to verify this works.
2:01:502 hours, 1 minute, 50 secondsOkay, and now I'm just going to click sign and pay and we're going to see what happens. Okay, cool, looks like we're here in the example sandbox. That's awesome. I'm just going to pump in some payment information here.
Chapter 15: Testing Payment Functionality
2:01:582 hours, 1 minute, 58 secondsCool, looks like the payment went through and then we also have this wonderful payment received button. You can close this window. That's awesome. Okay, great, so let's just adjust that final bit.
2:02:082 hours, 2 minutes, 8 secondsExcellent, everything worked great.
2:02:092 hours, 2 minutes, 9 secondsOn the final page where you do the confetti, make the confetti last a little bit shorter. The ones on the left and the right were a little long.
2:02:172 hours, 2 minutes, 17 secondsAnd then change will be in touch shortly to get started.
2:02:222 hours, 2 minutes, 22 secondsYou'll receive an email with more details and a link to book a kickoff call. Actually, screw that.
2:02:292 hours, 2 minutes, 29 secondsLet's just give them a direct calendar link to book a kickoff call. Why not? That's way easier and way faster. Okay, so I'm just going to give it my own calendar.
2:02:392 hours, 2 minutes, 39 secondsI'll just give it an example here and then boom, I'll just have it go off again. So I mean, this looks really clean so far. I guess there's one more thing I have to check. I have to check and see if we can see the proposals listed.
2:02:482 hours, 2 minutes, 48 secondsOkay, so yeah, we can. So can I click on this? Can we go right back to the page? Nice.
2:02:522 hours, 2 minutes, 52 secondsNow, can I just open this up in some new tab that's not logged in? Nice, so the slash P must be slash public. That's really clean. So I mean, I like this.
2:03:002 hours, 3 minutesWe did this in just a few minutes. Honestly, very sexy as you guys could see. I did very little work.
2:03:062 hours, 3 minutes, 6 secondsAnd yeah, I just need to find a way to basically standardize the spacing and the width.
2:03:112 hours, 3 minutes, 11 secondsI don't like how this one over here is on the left-hand side and then this stuff stretches all the way up to the right.
2:03:162 hours, 3 minutes, 16 secondsBut this is just a minor design thing and we can absolutely significantly upgrade this. God, we even have the signature here, which looks so cool. I love how that you can now build your own apps, right?
2:03:252 hours, 3 minutes, 25 secondsLike you don't actually have to go to like a big developer or pay out the ass for some big platform.
2:03:292 hours, 3 minutes, 29 secondsYou can just like one-shot an app like this with good enough Claude Code skills.
2:03:332 hours, 3 minutes, 33 secondsOkay, and it's gone through and it's updated the widths and stuff like that. That looks pretty clean.
2:03:372 hours, 3 minutes, 37 secondsNow I'm just gonna go give it some images and then it should be good. We're gonna add them to public slash images apparently. Nice, looking pretty clean if I do say so myself.
2:03:442 hours, 3 minutes, 44 secondsDon't know what the hell I was doing with cuffing my pants like that, but what are you gonna do? Just looking at what it changed.
2:03:502 hours, 3 minutes, 50 secondsIt made this a little bit wider, but then it made this much, much smaller.
2:03:532 hours, 3 minutes, 53 secondsSo I think what I'm gonna do is I'm just gonna enforce like the same width across the entire page. That probably makes the most sense.
2:03:582 hours, 3 minutes, 58 secondsWhy don't we just like constrain it so it'll be like here? I don't know, like here or something. That way it'll be somewhere in the middle.
2:04:062 hours, 4 minutes, 6 secondsJust gonna take a screenshot of this.
2:04:082 hours, 4 minutes, 8 secondsHey, this looks good, but I'm finding it a little too wide at the moment.
2:04:112 hours, 4 minutes, 11 secondsI believe we should just constrain it and do a bunch of padding on the left and the right. I sent you a screenshot of a quick example.
2:04:182 hours, 4 minutes, 18 secondsNow I guess we didn't actually do the screenshot, huh?
2:04:212 hours, 4 minutes, 21 secondsCause I mean, it's good cause it's like mobile optimized and stuff, but obviously, you know, on my actual desktop, there's just so much white space. Let's just center everything, make it scrollable.
2:04:312 hours, 4 minutes, 31 secondsAnd then I'm just going to, yeah, I'm not really sure why I couldn't take a screenshot of those, but whatever. That looks good to me.
2:04:392 hours, 4 minutes, 39 secondsBoom, just fed that in and we should be pretty good to go, I think. Holy, that salmon's good. I am definitely doing that again.
2:04:472 hours, 4 minutes, 47 secondsAnyway, I gave it some more time and it's centered most of this, I wanna say. Looks pretty clean, all things considered.
2:04:542 hours, 4 minutes, 54 secondsYou know, we're doing some cutting off of faces and whatnot, but it's not that bad.
2:04:572 hours, 4 minutes, 57 secondsAnd yeah, honestly, this is very similar to like the quality of a Panadoc.
2:05:002 hours, 5 minutesI guess the last thing I'm gonna do is I'm just gonna say, stretch the strategy bit all the way to the end. That probably makes the most sense.
2:05:102 hours, 5 minutes, 10 secondsStretch this bit all the way to the bounding boxes of the container, i.e. the white box should go all the way.
2:05:172 hours, 5 minutes, 17 secondsOkay, here's one more thing that I think this is a good opportunity to talk about.
Chapter 16: Leveraging GitHub for Project Management
2:05:212 hours, 5 minutes, 21 secondsA lot of the time, this will tell you to do things like create a GitHub repo, push the code, et cetera. Just ask the agent to do it.
2:05:272 hours, 5 minutes, 27 secondsMost of the time, it can actually do what it is that it's asking you to do.
2:05:312 hours, 5 minutes, 31 secondsIf it can, you know, let it try and then it'll tell you, absolutely. Hey, can you do all this for me?
2:05:382 hours, 5 minutes, 38 secondsAnd then it'll just tell you what parts it can do and which parts it can't. Okay, taking a peek here. It's telling me to go deploy the project.
2:05:462 hours, 5 minutes, 46 secondsSo go here, add new site, import an existing project. I can do that, select. And then we need to build settings.
2:05:542 hours, 5 minutes, 54 secondsShould auto-fill, confirm and click deploy. Proposal generator is available. That's funny. This is like the universal domain name, you're right.
2:06:022 hours, 6 minutes, 2 secondsLike anybody will be able to access this. I'll put an A at the end because I think it's funny.
2:06:112 hours, 6 minutes, 11 secondsOkay, automatically detected next. What else? Confirm, click deploy. Okay, so go to site settings. And then we need to add all this information in.
Chapter 17: Setting Up the Project
2:06:202 hours, 6 minutes, 20 secondsSo I'll do that. Environment variables, import from a .env file. So I'm just gonna paste this in. So there's this .env local.
2:06:292 hours, 6 minutes, 29 secondsLet me grab that. Okay, and we just imported all of these. Nice, oh, that's nice.
2:06:382 hours, 6 minutes, 38 secondsNow I need to go set up my Stripe webhook. So let's just paste that in. Add a destination. We need to add this endpoint URL.
2:06:472 hours, 6 minutes, 47 secondsSo proposal generator.
2:06:482 hours, 6 minutes, 48 secondsI don't know exactly what all this stuff means, but just gonna select all. And then I guess it's just proposal generator.
2:06:572 hours, 6 minutes, 57 secondsOkay, and then no description, I think. That looks good to me. Okay, everything is now added.
2:07:042 hours, 7 minutes, 4 secondsSo go through and then make sure my site's deployed. I saw some issue with it earlier. All right, so this is now gonna take whatever this is.
2:07:112 hours, 7 minutes, 11 secondsAnd now that it actually has access to the app, it should be able to update it for me. I don't know for sure, to be honest. We'll figure it out. Hopefully you guys can tell.
2:07:202 hours, 7 minutes, 20 secondsA lot of this stuff is me just saying, hey, fix it. And if it can't fix it, what the hell do I do?
2:07:252 hours, 7 minutes, 25 secondsAnd then it just tells you what to do, and then you're good to go.
2:07:282 hours, 7 minutes, 28 secondsWhat's important really is like, if you think about it, like this software engineering stuff, this is like almost completely automated.
2:07:332 hours, 7 minutes, 33 secondsI mean, I was doing more cooking of my salmon rice bowl than I was actually, you know, steering the ship after a certain point. And that's because we made use of the plan mode so heavily.
2:07:422 hours, 7 minutes, 42 secondsBut what's important really is like your agency as a developer and like your ideas and your willingness and capability to like put together things.
Chapter 18: The Power of Automation
2:07:512 hours, 7 minutes, 51 secondsIn my case, you know, I do a lot of proposals. I send out maybe one every couple of days right now. At our peak, we were sending like four or five out a day.
2:07:582 hours, 7 minutes, 58 secondsAnd so doing all that stuff manually was obviously very time intensive.
2:08:012 hours, 8 minutes, 1 secondWell, if I could just one shot it with like a little voice transcript and then an AI prompt, obviously, and then generate my own landing page, like that's really valuable for me as a business.
2:08:082 hours, 8 minutes, 8 secondsThat's something that AI would not know of right now and would not really be able to do. So, you know, allow the AI to be your hands.
2:08:172 hours, 8 minutes, 17 secondsYou, similar to the way that like, you know, keys and a keyboard are, you're the person that's coming up with the ideas and thinking.
2:08:222 hours, 8 minutes, 22 secondsOkay, so I'm not sure if you guys are paying attention while all of this is occurring, but did you see this little context tab get filled up?
2:08:302 hours, 8 minutes, 30 secondsBecause this is at 100% more than once at this point.
2:08:342 hours, 8 minutes, 34 secondsEssentially what occurs is this is your total amount of context available to you as somebody that's doing a build, right?
2:08:392 hours, 8 minutes, 39 secondsWell, when this reaches a certain limit, when it hits, you know, 99 or 100% or whatever, what it'll do is it'll take all of the texts that you've written so far, and it'll compress it down as tightly as humanly possible.
2:08:502 hours, 8 minutes, 50 secondsYou know, now let me commit and push.
2:08:512 hours, 8 minutes, 51 secondsSo Netlify Rebuilds might literally just turn into Netlify Rebuilding.
2:08:552 hours, 8 minutes, 55 secondsIt'll save all those tokens, but in doing so also increase the information density of your prompt. And then it'll basically compact it. That's what the term is.
2:09:042 hours, 9 minutes, 4 secondsSo that you have more information in the same amount of tokens.
2:09:082 hours, 9 minutes, 8 secondsSo the next prompt that you use is both higher quality, but then also doesn't actually run over the token limit.
2:09:142 hours, 9 minutes, 14 secondsThe unfortunate reality is models right now only have token limits of some more between 200,000 to about a million. Some of them have 200,000, other ones have a million. The model I'm currently using is about 200K right now.
2:09:222 hours, 9 minutes, 22 secondsAnd that means that like after 199,909 tokens go in, like there's only room for one more. That's just the way that they're built, right?
2:09:312 hours, 9 minutes, 31 secondsThat's just their infrastructure.
2:09:332 hours, 9 minutes, 33 secondsSo Claude does a lot of these like automated context management techniques without really telling you.
2:09:372 hours, 9 minutes, 37 secondsAnd that's core of what we're gonna learn after this project is done. Anyway, I went back and forth a couple of times. And now you can see that we have the app live. It's live on a public facing URL.
2:09:452 hours, 9 minutes, 45 secondsSo I'm gonna go in nickywicky at gmail.com and actually sign in with my previous account.
2:09:502 hours, 9 minutes, 50 secondsAnd now you can see, I actually have access to that same pipeline, that same page that I had previously. So I'm gonna give that a click.
2:09:572 hours, 9 minutes, 57 secondsEverything is nice and centered right now, which is exactly what I wanted. Super clean.
2:10:022 hours, 10 minutes, 2 secondsWhat's cool too is it stretched the strategy setup and fee all the way to the right-hand side.
2:10:062 hours, 10 minutes, 6 secondsAnd then, you know, I have the ability to do my signatures and whatnot. So suffice to say like this, this worked. This app is now functional. It's live.
2:10:132 hours, 10 minutes, 13 secondsIt's, you know, honestly probably better for my purposes than Pandidak was, which I was paying out the ass for.
2:10:182 hours, 10 minutes, 18 secondsNot that I don't think the company's cool, but damn is that some expensive API pricing, I think for what it's doing.
2:10:232 hours, 10 minutes, 23 secondsIn my case, I'm doing all that now basically for free, at least Netlify, you know, the deployment solution that I had available was free.
2:10:282 hours, 10 minutes, 28 secondsSo aside from the Claude code tokens, you know, it's one of those things where you spend it once and then every time I ever generate a proposal from here, I know, it's sort of fixed.
2:10:362 hours, 10 minutes, 36 secondsNow, I mean, we built like an app here, right? This is a full stack app. That's what this is. That's why there's like the login page. There's stuff on the backend. There's a database. There's, you know, the front end and whatnot as well.
2:10:452 hours, 10 minutes, 45 secondsBut I want you guys to know that like, despite Claude code and how awesome it is, I'd be very wary about taking apps that are fully vibe coded and then publishing them on the internet.
2:10:562 hours, 10 minutes, 56 secondsThis is sort of my obligatory safety message because there are people that are out there that are using Claude code and similar tools to try and find security vulnerabilities as well.
2:11:062 hours, 11 minutes, 6 secondsAnd unfortunately, despite how amazing Claude code is right now, it's not at the point where it like fully, 100% patches everything on the front end and the backend.
2:11:142 hours, 11 minutes, 14 secondsSo what this means is, okay, there are a couple little safety precautions that I recommend you have.
2:11:192 hours, 11 minutes, 19 secondsThe first is I'd recommend that whatever, you know, URL that you're putting together or whatnot, it's not like an obvious or basic URL.
2:11:272 hours, 11 minutes, 27 secondsLike for instance, I wouldn't just go proposal generated on elified.app-router.
2:11:312 hours, 11 minutes, 31 secondsI actually have my custom URL and then I'd make the custom URL something that, you know, realistically is not like trivial. It wouldn't be like google.com, right?
2:11:392 hours, 11 minutes, 39 secondsLike not left click.ai.
2:11:402 hours, 11 minutes, 40 secondsI wouldn't make it short because there are a bunch of services out there that are scanning all DNS ranges and also all URLs, which basically mean that like the shorter
2:11:482 hours, 11 minutes, 48 secondsand simpler your thing is, the riskier it is, the more other human beings will have access to this.
2:11:512 hours, 11 minutes, 51 secondsLike there's probably already been, I don't know, like 30 or 40 people that have accessed my service despite the fact that I just whipped it up. That's just how it works, right?
2:11:582 hours, 11 minutes, 58 secondsPeople are always constantly scanning the internet and sending requests.
2:12:012 hours, 12 minutes, 1 secondThe second is I wouldn't charge money for these, okay, without having a developer go through the authentication, at least the front end, at least once.
2:12:112 hours, 12 minutes, 11 secondsAnd I say this for liability reasons, like I don't want you guys to like get a bunch of user data, like usernames, passwords, email addresses,
2:12:192 hours, 12 minutes, 19 secondspayment logs and stuff like that, and then have that exposed to bad actors on the internet. It just isn't really worth it right now.
2:12:242 hours, 12 minutes, 24 secondsLike if you guys are looking to sell apps with this approach, you know, just pay some person, you know, a few hundred dollars, have them look over your app, let's be real, the software is not the mode anyways.
2:12:332 hours, 12 minutes, 33 secondsYou can just give it to them, screw the NDA. And just like have them tell you how to secure your application.
2:12:382 hours, 12 minutes, 38 secondsHell, they can even give Claude Code some tips or maybe like a prompt that you could use to do it almost automatically.
2:12:442 hours, 12 minutes, 44 secondsBut I guess what I'm trying to say is like, despite how compelling it may be to like make these apps public and stuff like that, and then charge people for their usage, I personally wouldn't.
2:12:512 hours, 12 minutes, 51 secondsI personally only use apps right now, internally within my teams or for my clients.
2:12:562 hours, 12 minutes, 56 secondsI do not roll these things out and then like try and make money from them off the wider internet, when like the app store or whatever. I've just seen too many horror stories.
2:13:052 hours, 13 minutes, 5 secondsWe saw Cloudbot a couple of weeks ago, at least at the time of this recording, which later turned into Moltbot, which later turned into OpenCloud.
2:13:122 hours, 13 minutes, 12 secondsIt rebranded 5 million times because every fricking version of it had major security issues.
2:13:172 hours, 13 minutes, 17 secondsAnd then people were getting prompt injected and hacked and stuff like that.
2:13:202 hours, 13 minutes, 20 secondsAnd I mean, like, you know, there's a fair amount of reputation that goes with that as somebody in a business context, but also you are playing with fire here. This is like, you know, real human beings, consumer data.
2:13:302 hours, 13 minutes, 30 secondsSo I don't want to make safety too big a part of my thing. It's just Uncle Ben time. With great power comes great responsibility.
2:13:362 hours, 13 minutes, 36 secondsAnd hopefully you guys see here, I mean, this took me, I don't know, 15, 20 minutes, realistically end to end. I was obviously making food and whatnot coming back. I wasn't as efficient as I could have been.
2:13:442 hours, 13 minutes, 44 secondsBut you are certainly wielding great power right now.
2:13:462 hours, 13 minutes, 46 secondsAnd if you're going to have other people trust you with their credentials and logins and passwords and everything like that, you need to make sure that, you know, you're not using that power willy nilly.
Chapter 19: Context Management Explained
2:13:542 hours, 13 minutes, 54 secondsNext up, I want to chat context management.
2:13:572 hours, 13 minutes, 57 secondsNow, for those of you guys that don't know, context management is essentially you handling tokens in a prompt as effectively as possible.
2:14:062 hours, 14 minutes, 6 secondsThere are many people out there that overcomplicate the hell out of this. So I'm going to do my best not to.
2:14:112 hours, 14 minutes, 11 secondsIf I open up a new instance of Claude Code over here, and then I type this backslash and then scroll down, you'll see that I have access to a bunch of really cool functions here.
2:14:212 hours, 14 minutes, 21 secondsI can compact context, cost, debug, init. I can do insights.
2:14:262 hours, 14 minutes, 26 secondsI have the ability to choose between models, thinking, account and usage, fast mode.
2:14:312 hours, 14 minutes, 31 secondsWe're going to talk all about this next, but for now I want to focus specifically on one slash command called slash context.
2:14:392 hours, 14 minutes, 39 secondsLook at what happens when I click this.
2:14:422 hours, 14 minutes, 42 secondsIf I scroll up and then zoom in a little bit, you could see here that at the very top, cloud tells us essentially what is currently using its context window.
2:14:522 hours, 14 minutes, 52 secondsFor those of you guys that don't know, context window in the domain of AI just refers to the total amount or total number of tokens that a specific model can deal with at once.
2:15:042 hours, 15 minutes, 4 secondsSo if you guys remember earlier where we were doing a build, I said it was about 200,000 for cloud Opus 4.6. That's the model that I'm currently using.
2:15:112 hours, 15 minutes, 11 secondsThere's some models out there like some sonnet series models that can go up to one or 2 million tokens now, but the number of tokens in a context window aren't directly related to the performance of the model.
2:15:212 hours, 15 minutes, 21 secondsContext window is sort of separate from that. So cloud Opus 4.6 has a context window of about 200,000 tokens.
2:15:272 hours, 15 minutes, 27 secondsAnd then you'll see here that so far I've used 26,400, which means mathematically I'm 13% of the way through.
2:15:352 hours, 15 minutes, 35 secondsYou might be asking, well, Nick, how the hell is that possible? All you've written so far is slash context.
2:15:412 hours, 15 minutes, 41 secondsWhere are those other 26,398 tokens coming from realistically? And that's a great question. Immediately underneath, you could find out for yourself.
2:15:502 hours, 15 minutes, 50 secondsAnd so what I reckon you guys do right now, if you've never done this before, is head over to your own Claude Code instance without even watching any of this and just type backslash context
2:15:582 hours, 15 minutes, 58 secondsand look at all of the things that are currently consuming your prompt.
2:16:022 hours, 16 minutes, 2 secondsNow I should note that this is stuff that you're actively billed for. This is not stuff that's free.
2:16:072 hours, 16 minutes, 7 secondsDespite the fact that a lot of the time Anthropic and Cloud, they'll add a bunch of things to your context that really telling you, this is still stuff that at the end of the day you are paying for.
2:16:162 hours, 16 minutes, 16 secondsSo if you submit a bunch of one-off requests to like individual instances of Cloud, note that there's gonna be your prompt, which it'll bill you for, but there's also gonna be always like a flat additional cost
2:16:252 hours, 16 minutes, 25 secondsof maybe five, 10, 15,000 tokens or more depending on how you set it up.
2:16:302 hours, 16 minutes, 30 secondsOkay, so going down here under category, you could see all of the different ways that our tokens are currently being used and all of the additional tokens that we didn't really realize were making use of.
2:16:392 hours, 16 minutes, 39 secondsThe first is your system prompt.
2:16:412 hours, 16 minutes, 41 secondsNow, if you guys remember, cloud.md takes up a fair amount of your context. And there's different types of cloud.mds.
2:16:492 hours, 16 minutes, 49 secondsYou have your global tilde slash dot cloud slash.
2:16:542 hours, 16 minutes, 54 secondsThat's the one that defines all workspaces, not just the one that you're currently in. Then you have the local dot cloud right over here in yellow.
2:17:022 hours, 17 minutes, 2 secondsIn this case, we've broken them down into individual rule or component cloud.mds. Underneath, you also have capital memory.md.
2:17:102 hours, 17 minutes, 10 secondsAnd then, and only then do you actually send a message basically and have your prompt.
2:17:152 hours, 17 minutes, 15 secondsAnd so earlier on, you remember how we had like 26,000 tokens or so?
2:17:182 hours, 17 minutes, 18 secondsWell, probably, I don't know, 10,000 tokens or something like that was just taken up by all these system prompts.
2:17:242 hours, 17 minutes, 24 secondsWe'll double check in a second because we can actually see the real number.
2:17:272 hours, 17 minutes, 27 secondsAnd then only a couple of tokens, in this case two or something, were actually taken up by our other request.
2:17:332 hours, 17 minutes, 33 secondsSo that begs the question, where are the other, I guess, 15,000 or so tokens of the 26,400?
2:17:382 hours, 17 minutes, 38 secondsIn addition to the system prompt, which to be clear, this is your cloud.md. And rules.
2:17:462 hours, 17 minutes, 46 secondsYou also have system tools, which is as of the time of this recording, almost 17,000 tokens.
2:17:542 hours, 17 minutes, 54 secondsNow system tools are things like the model's ability to run bash. That just means open up a terminal.
2:18:012 hours, 18 minutes, 1 secondIt's the model's ability to run web search.
2:18:042 hours, 18 minutes, 4 secondsThat means to request a webpage, have that webpage information brought back, parsed, and then dealt with.
2:18:112 hours, 18 minutes, 11 secondsIt's the model's ability to do things like create a plan.
2:18:162 hours, 18 minutes, 16 secondsThese are all tools and functions that you don't actually realize that Cloud has access to, but it does.
2:18:222 hours, 18 minutes, 22 secondsAnd this is what the Claude Code developers, Boris, Cherny, and all the rest of the team have basically done.
2:18:282 hours, 18 minutes, 28 secondsBefore you even get to your own message, which is all the way down over here, as we see, we have that cloud.md stuff.
2:18:362 hours, 18 minutes, 36 secondsThen we have the tools. Then we have MCP, which I'll cover in a second. Then we have that memory.md. Then we have skills.
2:18:452 hours, 18 minutes, 45 secondsAnd then, and only then, do we actually have our messages. So there's a lot to go yet. These tools are constantly changing.
2:18:512 hours, 18 minutes, 51 secondsAnd if you guys want a list of all of them, you can actually just ask your Cloud model.
2:18:552 hours, 18 minutes, 55 secondsSo I'm just gonna say, what tools do you have access to list them all?
2:18:582 hours, 18 minutes, 58 secondsAnd it's gonna go through, and it's going to enumerate every single one. So you see here, we have task. That's what opens up every time we call a sub-agent.
2:19:062 hours, 19 minutes, 6 secondsThere's task output, which is another tool where it retrieves the output of the agent. There's bash, which is how you execute shell commands. Glob is finding a file by pattern.
2:19:162 hours, 19 minutes, 16 secondsGrep is searching file contents. Read is just how it reads files. You do need an additional tool for that. Edit is how it changes things.
2:19:232 hours, 19 minutes, 23 secondsWrite is how it creates and overwrites new files.
2:19:262 hours, 19 minutes, 26 secondsNotebook edit is something specific for a type of file called a Jupyter Notebook.
2:19:302 hours, 19 minutes, 30 secondsA lot of people do like data science and stuff like that in Claude Code. And Jupyter Notebooks are a big chunk of that.
2:19:352 hours, 19 minutes, 35 secondsThere's web-fetch, which is how it calls various internet sources and then returns it.
2:19:392 hours, 19 minutes, 39 secondsThis is web search, which allows it to search sort of like Google. There's to-do write.
2:19:442 hours, 19 minutes, 44 secondsIf you guys have ever wondered where those little to-do lists come up when Claude Code is doing stuff, it's that one right over there. Ask user question.
2:19:512 hours, 19 minutes, 51 secondsIf you guys have ever wondered where those little graphical user interfaces come up where it says pick one, two, three, or tell Cloud something, that's where that comes from. There's enter plan mode, exit plan mode.
Chapter 20: Understanding MCP Tools
2:20:002 hours, 20 minutesThere's skill, which is just a meta-function which more or less orchestrates how you call skills.
2:20:052 hours, 20 minutes, 5 secondsThen there's task stop, which is useful because sometimes Cloud needs to stop something that's running.
2:20:092 hours, 20 minutes, 9 secondsOkay, so basically of all of the context, we scroll back up here and avoid this MCP tools. We'll cover that in a second. Okay.
2:20:172 hours, 20 minutes, 17 seconds16,800 tokens are being taken up by all those tools, basically all of the time.
2:20:222 hours, 20 minutes, 22 secondsAnd there's nothing you can do to fix that unless you wanna go in and make your own version of Claude Code or something.
2:20:272 hours, 20 minutes, 27 secondsI will say, I think that some of these things are unnecessary. I mean, I definitely don't need the Jupyter Notebook calls.
2:20:332 hours, 20 minutes, 33 secondsI think there are a few additional features here that maybe I don't need or we could probably make them smaller.
2:20:362 hours, 20 minutes, 36 secondsBut this is something that the Claude Code team is constantly improving, constantly pruning and so on and so forth. Next up, we have the MCP tools.
2:20:452 hours, 20 minutes, 45 secondsNow, unlike system tools, MCP tools are things that you define yourself, which means every one of these tools is something that I basically put together.
2:20:522 hours, 20 minutes, 52 secondsThis is something that I connected to an MCP server to basically extend the functionality of my Claude Code.
2:20:592 hours, 20 minutes, 59 secondsSo basically what I'm trying to say is these right here are default and these ones right over here, you control.
2:21:052 hours, 21 minutes, 5 secondsAnd so, as a percentage of my total context, I'm spending 2.8% on customizing my own Cloud instance and then 8.4% with just the default.
2:21:122 hours, 21 minutes, 12 secondsObviously the default ones are a lot bigger, but some of these MCP tools can be pretty valuable.
2:21:162 hours, 21 minutes, 16 secondsIssues with some MCP tools are, they're really, really big, as you guys are gonna see when I screw around with a couple of crappy libraries. So you have to be pretty selective about how you choose them.
2:21:252 hours, 21 minutes, 25 secondsAnd that's what this next section is down here called MCP tools.
2:21:292 hours, 21 minutes, 29 secondsSo for instance, I downloaded an MCP, a Model Context Protocol Toolkit called Chrome DevTools. This just allows Claude to open my browser.
2:21:382 hours, 21 minutes, 38 secondsSo what I could do is I could say, hey, open a Chrome instance and go to nixorayef.com.
2:21:432 hours, 21 minutes, 43 secondsIf you think about it, the context that I put together here, let me change this and say, great work. Go to leftclick.ai.
2:21:532 hours, 21 minutes, 53 secondsIf you think about it, immediately above each of my messages is obviously all of the tools, right?
2:21:582 hours, 21 minutes, 58 secondsAnd so what these tools are, is they're basically definitions that say, hello Claude, you have access to the ability to take a screenshot.
2:22:032 hours, 22 minutes, 3 secondsIf you wanna take a screenshot, just call this specific tool and it'll do the screenshotting for you.
2:22:082 hours, 22 minutes, 8 secondsAnd so this is all above sort of my initial prompt where I say, great work, go to leftclick.ai.
2:22:132 hours, 22 minutes, 13 secondsAnd so when I say go to leftclick.ai, Claude knows, okay, like earlier on it said, if a user asks you to go to a website, call this tool, it obviously just references the specific thing.
2:22:212 hours, 22 minutes, 21 secondsAnd as you guys could see here, it's navigating, it's taking screenshots and it's basically controlling my browser right now, which is really cool.
2:22:272 hours, 22 minutes, 27 secondsSo that's an example of a tool that I think is pretty valuable.
2:22:312 hours, 22 minutes, 31 secondsThat said, there are a lot of tools that aren't super valuable.
2:22:332 hours, 22 minutes, 33 secondsAnd unfortunately, MCP tends to consume a fair amount of your context if you're not careful.
2:22:382 hours, 22 minutes, 38 secondsAs you see here, there's click, close page, drag, emulate, evaluate script, fill, fill form, and so on and so forth.
2:22:432 hours, 22 minutes, 43 secondsI'm not gonna cover all these because there's just so many different MCPs that you could use and each of them have so many different tools. Underneath that, you have memory files.
2:22:492 hours, 22 minutes, 49 secondsYou guys remember earlier when I told you that there was this big like M-O-M-O-R-Y.M-D, that memory is super straightforward.
2:22:582 hours, 22 minutes, 58 secondsAnd in our case, that's only 88 tokens, not that big of a deal, but it's basically Claude Scratchpad as it works. Next you have skills.
2:23:042 hours, 23 minutes, 4 secondsIf you guys remember, we had a Claude skills folder in another repo.
2:23:102 hours, 23 minutes, 10 secondsThat Claude skills folder basically in our case, like, I don't know, browsed Amazon and found something for us. In this one, we don't.
2:23:162 hours, 23 minutes, 16 secondsSo the only thing that's really storing is just the skills definition, which in this case is 61 tokens, expressed as a fraction of the total number of tokens we have available to us, 200,000.
2:23:252 hours, 23 minutes, 25 secondsYou could see that that's not even 0.1%.
2:23:282 hours, 23 minutes, 28 secondsBy the way, as I've communicated and kept talking with Claude, we've accumulated more tokens. So you can see how earlier it was at like 24,000. Well, now we're at 30,600, right?
2:23:372 hours, 23 minutes, 37 secondsWe've gone up from, I think, 13% to 15%. So that'll continue happening as we go on. Next up, of course, you have your messages.
2:23:432 hours, 23 minutes, 43 secondsAnd so in our case, we're consuming 2.6% of our entire context window right now, just through messages and just through back and forth.
2:23:492 hours, 23 minutes, 49 secondsThis is sort of inescapable or unavoidable, although there are ways to manage your context a lot more efficiently.
2:23:542 hours, 23 minutes, 54 secondsA couple of ways are speak in high information density ways wherever possible.
2:23:592 hours, 23 minutes, 59 secondsObviously, voice transcript tools are kind of against that because they take into account all of your ums and ahs and whatnot.
2:24:042 hours, 24 minutes, 4 secondsBut if you want it to be really, really efficient, what you would do is you would take your voice transcript, pump it into a cheaper model, one that doesn't cost you as much money that's in a separate tab,
2:24:122 hours, 24 minutes, 12 secondshave that summarize it into a very tight request, and then actually send that to the initial Claude agent.
2:24:182 hours, 24 minutes, 18 secondsAnd that's a strategy that I've used to manage really small context windows in the past.
2:24:222 hours, 24 minutes, 22 secondsObligatory, this is where the conversation with Claude actually occurs.
2:24:252 hours, 24 minutes, 25 secondsI should note that, I don't know if you guys remember, but sometimes you can ask Claude stuff and then a little thinking tab will pop up.
2:24:312 hours, 24 minutes, 31 secondsWell, that thinking tab isn't actually included in the messages.
2:24:332 hours, 24 minutes, 33 secondsYou are still billed for this separately, but basically what happens is at the time that you make a request and at the time that the thinking occurs, it sticks all that onto the big message chain and then it uses that to figure out the next thing.
2:24:442 hours, 24 minutes, 44 secondsSo it uses basically this thinking area, it's almost a scratch pad to figure out more.
2:24:482 hours, 24 minutes, 48 secondsAnd then what it does is it collapses it, disappears it, and then it just gives you the answer.
2:24:532 hours, 24 minutes, 53 secondsAnd then it pretends as if the reasoning or thinking little section didn't even exist.
2:24:572 hours, 24 minutes, 57 secondsSo don't worry about thinking here, assuming you have extended thinking on, that doesn't really get included, although of course you are still paying for it.
2:25:052 hours, 25 minutes, 5 secondsAnd then finally, the bulk of our context is free space, 67.7%, which is good for us. Not really sure why they include that here, but they do.
2:25:122 hours, 25 minutes, 12 secondsThe last thing that you guys need to understand is this idea of an auto compact buffer.
2:25:152 hours, 25 minutes, 15 secondsNow an auto compact buffer is basically just a certain amount of space that Claude's developers always leave available.
2:25:202 hours, 25 minutes, 20 secondsAnd then basically what happens is when you hit that buffer, aka when there's only 33,000 tokens left, it'll automatically compact all of the previous conversation history.
2:25:302 hours, 25 minutes, 30 secondsNow this is done automatically, but you can also do this manually by going slash compact.
2:25:342 hours, 25 minutes, 34 secondsWhat happens when you go slash compact is it basically takes all of our conversation history here. It'll take this, it'll take that, it'll take all that.
2:25:422 hours, 25 minutes, 42 secondsAnd then it'll just squash it down into a very high information density summary.
2:25:462 hours, 25 minutes, 46 secondsAnd so what I'm gonna do is immediately after it compacts, I'm actually gonna ask it to tell me what it just compacted.
2:25:502 hours, 25 minutes, 50 secondsBasically, hey, tell me what is currently available in your context.
2:25:542 hours, 25 minutes, 54 secondsOkay, and as you can see here, it says this session is being continued from a previous conversation that ran out of context.
2:25:582 hours, 25 minutes, 58 secondsThe summary below covers the earlier portion of the conversation.
2:26:012 hours, 26 minutes, 1 secondFirst, there's an analysis tab where it chronologically analyzes your conversation.
2:26:052 hours, 26 minutes, 5 secondsFirst message user ran this, then user asked this, then user asked this, then user asked this, user asked this, user asked this, and so on and so forth.
2:26:132 hours, 26 minutes, 13 secondsAnd so if you compare all of this to all of the messages and all of the tool calls and everything like that we did above, this will be fewer tokens, right?
2:26:212 hours, 26 minutes, 21 secondsSignificantly so, probably like three or four X.
2:26:242 hours, 26 minutes, 24 secondsAnd so in this way, gradual and progressive compaction of a conversation maximizes the information density.
2:26:302 hours, 26 minutes, 30 secondsAnd then Claude's really good at like not leaving important things out.
2:26:342 hours, 26 minutes, 34 secondsSo you tend to have most of the information that you really want or really need in this context.
2:26:392 hours, 26 minutes, 39 secondsThey've started also recently doing something called auto-compaction, which is where this compaction is occurring constantly in the background for you, AKA your oldest messages are just like compacted into higher information density summaries.
2:26:502 hours, 26 minutes, 50 secondsAnd then that's constantly sort of like a tail behind your current conversation.
2:26:532 hours, 26 minutes, 53 secondsThat's pretty cool because if you think about it, us human beings are just not very good at remaining really concise and being very precise and constantly updating that context improves the quality
2:27:032 hours, 27 minutes, 3 secondsof subsequent outputs, as well as bills you less, which is kind of interesting because Anthropexol business model right now is monetizing Claude and Claude and Claude code.
2:27:142 hours, 27 minutes, 14 secondsSo the fact that they're doing this sort of runs contrary to their interests, which is one of the reasons why I like them as a company.
2:27:182 hours, 27 minutes, 18 secondsThey're obviously motivated by the quality of their product more necessarily than their revenue and whatnot. So Claude's website here is really helpful.
Chapter 21: Strategies for Token Management
2:27:252 hours, 27 minutes, 25 secondsThey have actually a whole section dedicated to reducing token usage and minimizing the amount of what's called context rot that accumulates in a conversation.
2:27:332 hours, 27 minutes, 33 secondsI'm just gonna run through them with you guys. And I want you to know this is constantly being updated.
2:27:372 hours, 27 minutes, 37 secondsSo the time that you look at it might be a little bit different from the time that I'm looking at it.
2:27:402 hours, 27 minutes, 40 secondsAgain, if you guys want like up to date tips, I recommend checking out Twitter, x.com, talking to Grok and just saying, summarize the best strategies
2:27:482 hours, 27 minutes, 48 secondsto reduce token usage that users have been talking about in the last month or so.
2:27:512 hours, 27 minutes, 51 secondsSo you have strategies like RAG, retrieval augmented generation, you have strategies like continuously and consistently compressing the Claude.md.
2:27:582 hours, 27 minutes, 58 secondsYou have strategies like telling Claude to write as concisely as possible, but then turning on extended thinking, which is a feature I'll run you guys through later,
2:28:062 hours, 28 minutes, 6 secondswhich basically means you blow up the reasoning tokens, but then the actual token spill ends up being very low and so on and so forth.
2:28:132 hours, 28 minutes, 13 secondsSo the number one recommendation that they have is to manage your contacts proactively by using slash cost. This helps you check your current token usage.
2:28:202 hours, 28 minutes, 20 secondsYou can also configure a status line to display continuously.
2:28:232 hours, 28 minutes, 23 secondsIn order to configure a status line in Claude code, you just go slash status line.
2:28:272 hours, 28 minutes, 27 secondsYou'll notice that you can't currently do this inside of the GUI version.
2:28:302 hours, 28 minutes, 30 secondsSo what you have to do instead is you have to open Claude in a terminal here because, you know, graphical user interfaces don't have a status line.
2:28:372 hours, 28 minutes, 37 secondsA status line is basically this little piece of text that occurs before.
2:28:402 hours, 28 minutes, 40 secondsAnd then you just go slash status, sorry, slash status line here. It'll ask you what you wanna put in your status line.
2:28:482 hours, 28 minutes, 48 secondsAnd so I'm basically just gonna ask it to include a little like loading bar with the total number of tokens that I've used.
2:28:562 hours, 28 minutes, 56 secondsUpdate my status line so it includes a little loading bar that is how many tokens that I've used out of my total context.
2:29:022 hours, 29 minutes, 2 secondsSo as you could see, it converted that into another mini prompt using a status line setup agent, and then it's gonna do this kind of cool little status effect.
2:29:102 hours, 29 minutes, 10 secondsSo I'm actually gonna get to see it down here. I'll show you guys what that looks like in a sec.
2:29:132 hours, 29 minutes, 13 secondsOkay, and as we could see here, we now have that little bar. So 13% of my tokens are used up. That's kind of neat. We also see the current model.
2:29:212 hours, 29 minutes, 21 secondsAnd then, you know, the branch that we're on if you're into programming with repositories and like get workflows and stuff like that.
2:29:272 hours, 29 minutes, 27 secondsThis to me isn't like super valuable to be honest, if I'm being frank, I just thought it was kind of cool.
2:29:312 hours, 29 minutes, 31 secondsSo just another reason why doing all of this in the terminal gives you significantly more latitude.
2:29:362 hours, 29 minutes, 36 secondsYou can't really just like add a status line to the GUI version, at least not now, but this one's very hackable.
2:29:402 hours, 29 minutes, 40 secondsAnother thing you can do is you can add custom compaction instructions.
2:29:442 hours, 29 minutes, 44 secondsSo you can actually say slash compact and then give it a prompt telling it what to prioritize.
2:29:482 hours, 29 minutes, 48 secondsYou could do this every now and then, which is obviously quite valuable.
2:29:522 hours, 29 minutes, 52 secondsYou can use slash clear to start fresh when switching to something that is unrelated.
2:29:562 hours, 29 minutes, 56 secondsSo what I mean by this is if I just delete this little terminal instance down here, just go back slash clear, what it'll do is it'll clear the entire conversation so you have no context anymore.
2:30:042 hours, 30 minutes, 4 secondsSo now there's no previous context.
2:30:062 hours, 30 minutes, 6 secondsIf I go back to slash context, you can see that scrolling up to messages, we have 152 tokens, which is basically everything that we've done here so far.
2:30:132 hours, 30 minutes, 13 secondsAside from that, you can use instructions inside of the Clawed-down MD to basically try and minimize the total number of tokens generated, as I mentioned.
2:30:202 hours, 30 minutes, 20 secondsSo you could say something like, hey, write as succinctly as possible.
2:30:232 hours, 30 minutes, 23 secondsYou can reason all that you want because that isn't added to the context, but when you actually give me something, just give me the bare bones information. If I need more, I'll actually ask you.
2:30:302 hours, 30 minutes, 30 secondsChoosing the right model is really big.
2:30:322 hours, 30 minutes, 32 secondsSo if you're using a really simple sub agent or something, we'll talk about how to develop those later on. I recommend using smaller models like Sonnet.
2:30:392 hours, 30 minutes, 39 secondsThese smaller models are typically less intelligent, but they have much larger context windows and then you'll build less.
2:30:432 hours, 30 minutes, 43 secondsSo that allows you to like do all the heavy lifting inside of the sub agent that's cheaper, and then they just return you those results, which is great.
2:30:492 hours, 30 minutes, 49 secondsAs you see here, the Anthropic team specifies to reduce the MCP server overhead.
2:30:532 hours, 30 minutes, 53 secondsAnd that's because, as I mentioned to you guys, some MCP servers just suck. They just have a bajillion tools.
2:30:582 hours, 30 minutes, 58 secondsYou'll download one and 20% of your token usage will be gone immediately.
2:31:022 hours, 31 minutes, 2 secondsThat's obviously quite costly, and then it makes Claude much dumber. So, you know, there are ways to reduce MCP server overhead.
2:31:072 hours, 31 minutes, 7 secondsThey have what's called advanced and automatic tool search now.
2:31:102 hours, 31 minutes, 10 secondsWhen MCP tool descriptions exceed 10% of your context window, they won't actually load all of them. They'll just try and search for them before.
2:31:162 hours, 31 minutes, 16 secondsSo meaning you'll say, hey, you know, can you open up a new page in Chrome DevTools or something? It won't actually have access to all that immediately.
2:31:232 hours, 31 minutes, 23 secondsWhat it'll do is it'll search first a list of tools using Gratper or something like that, which is its own built-in search tool.
2:31:292 hours, 31 minutes, 29 secondsAnd then it'll find one that says open the Chrome DevTools and then it'll load it.
2:31:322 hours, 31 minutes, 32 secondsThat helps you avoid massive MCP server overheads and then obviously wasting a lot of tokens.
2:31:372 hours, 31 minutes, 37 secondsSome other tips are to move instructions from Claude MD to skills.
2:31:402 hours, 31 minutes, 40 secondsSo remember how earlier I said your Claude MD should be like 200 to like four or 500 tokens, lines maximum. Some people make it even longer, but you shouldn't.
2:31:492 hours, 31 minutes, 49 secondsInstead, what you can do is you can break those down into specific rules.
2:31:512 hours, 31 minutes, 51 secondsAnd then any rules that are more tasks than rules, you can actually just turn into skills.
2:31:562 hours, 31 minutes, 56 secondsSo skills will load on demand, meaning that only when you specifically invoke them will they be added to your context, which is quite helpful. You can also adjust extended thinking.
2:32:042 hours, 32 minutes, 4 secondsWe haven't chatted too much about extended thinking, but if you go back to Claude here, if you go to slash model, you have the ability to switch which model you're using.
2:32:122 hours, 32 minutes, 12 secondsAnd then additionally, if you go over here, there's also a thinking tab, which allows you to turn on and off that little reasoning or thinking window.
2:32:202 hours, 32 minutes, 20 secondsAs I mentioned, thinking is pretty valuable because it avoids you wasting tons of tokens in the conversation chain itself. It offloads it to a little thinking tab.
2:32:272 hours, 32 minutes, 27 secondsAnd what you can do is you can actually modify the effort level using slash model.
2:32:312 hours, 32 minutes, 31 secondsYou can disable thinking completely, or you can turn the number of tokens that you give it to like maximum think from, I don't know, 8,000 to like 32,000 or more.
2:32:412 hours, 32 minutes, 41 secondsNow agent teams is another feature of Claude code, which I'm looking forward to covering with you guys, but currently it costs a lot, about seven times more tokens than standard sessions,
2:32:482 hours, 32 minutes, 48 secondsespecially when teammates run in plan mode, because every teammate maintains its own context window.
2:32:522 hours, 32 minutes, 52 secondsSo they actually kind of recommend against it if minimizing token usage is the number one thing that you wanna do.
2:32:572 hours, 32 minutes, 57 secondsAnd then finally, writing specific prompts is probably the highest ROI tip that I could give you here.
2:33:032 hours, 33 minutes, 3 secondsInstead of improve this code base, you saying something specific, hey, fix this one feature that I found in this file is a lot more precise.
2:33:102 hours, 33 minutes, 10 secondsAnd as a result, despite the fact that it'll, it takes a little bit more thinking on your end, a little bit more of your extended thinking, Claude's token usage ends up being significantly decreased.
2:33:192 hours, 33 minutes, 19 secondsFinally, even Anthropic says that planning for complex tasks is the way to go because this significantly reduces the total number of tokens you use when you're actually building solutions.
2:33:282 hours, 33 minutes, 28 secondsUsually API calls and calling servers and requesting web pages and stuff, these load a ton of tokens in the context. So avoiding doing research entirely is pretty valuable.
2:33:372 hours, 33 minutes, 37 secondsOnce you're at the building stage, just front load the research with plan and worry about it later.
2:33:412 hours, 33 minutes, 41 secondsNow it's time to chat skills, which in my opinion is probably one of the most economically valuable ways that you can use Claude code. This is a Claude code in aggregate tutorial, obviously.
2:33:512 hours, 33 minutes, 51 secondsSo I don't just wanna talk about skills.
2:33:522 hours, 33 minutes, 52 secondsIf you guys wanna know more about how I personally use skills and things like skills, I do also have another course that talks all about what I call agentic workflows, which are analogous to skills.
2:34:022 hours, 34 minutes, 2 secondsBut for now, anybody that's not acquainted with this, I just wanna run a quick demo.
2:34:052 hours, 34 minutes, 5 secondsSo if we open up this .Claude folder in the top left-hand side, you can see that we also have a nested skills folder. And I have a bunch of different skills here.
2:34:132 hours, 34 minutes, 13 secondsI have skills that allow me to classify leads, create proposals automatically, not dissimilar to the proposal generator app that we did before.
2:34:212 hours, 34 minutes, 21 secondsI find outliers in my niche, update and auto-reply to emails, edit my YouTube videos for me,
2:34:292 hours, 34 minutes, 29 secondsonboard new clients to my agency, apply to Upwork jobs and so on and so forth, monitor and then classify my school posts.
2:34:362 hours, 34 minutes, 36 secondsI mean, if you think about it, what this is, is this is a collection of all the things that I usually do in a daily basis, like for my own intellectually valuable knowledge work, the stuff that I basically get paid for.
2:34:452 hours, 34 minutes, 45 secondsAnd then what I've done is I've just turned them into checklists, and then I've just given these checklists over to Claude. So let's pretend that I wanna do one of these tasks today.
2:34:542 hours, 34 minutes, 54 secondsIn my case, I want to scrape some leads.
2:34:572 hours, 34 minutes, 57 secondsSo what I've done is I've created a skill up here called scrape leads that scrapes and verifies business leads using a service.
2:35:022 hours, 35 minutes, 2 secondsThen it classifies with a large language model, enriches the emails and saves it to a Google Sheet.
2:35:072 hours, 35 minutes, 7 secondsUse when the user asks to find leads, scrape businesses, generate prospect lists, or build lead databases for any industry or location.
2:35:122 hours, 35 minutes, 12 secondsI then have a goal up top, which is to scrape leads using a particular source. I have a bunch of inputs. I even have some scripts that I could use to run these. And then I have a process, and this is my checklist.
2:35:212 hours, 35 minutes, 21 secondsStart with a test scrape, then do verification, then do a full scrape, then do LLM classification, upload to a Google Sheet, enrich missing emails, and so on and so on and so forth.
Chapter 22: Creating Skills for Efficiency
2:35:302 hours, 35 minutes, 30 secondsOkay, so as you see here, big, big deal.
2:35:322 hours, 35 minutes, 32 secondsThis is a fair amount of time and energy that I used to take to do these lead scraping things.
2:35:362 hours, 35 minutes, 36 secondsAs part of my work is both for my dental company and then for LeftClick, on behalf of my clients, lead scraping is like a major chunk of what makes a successful cold email campaign.
2:35:472 hours, 35 minutes, 47 secondsAnd I just had to do it myself every time. It took like an hour or two.
2:35:502 hours, 35 minutes, 50 secondsWell, what I can do now is I can just turn all of my own knowledge into a skill.
2:35:542 hours, 35 minutes, 54 secondsOkay, I can define it in Markdown format here, and I can write it with Claude, and then I can just say, scrape me 1K dentists, or 1,000 dentists across the United States.
2:36:062 hours, 36 minutes, 6 secondsNow, when I press this button, what's happening now is it's successfully loading the skill.
2:36:112 hours, 36 minutes, 11 secondsIt's starting with a test scrape of 25 dentists to verify my quality.
2:36:152 hours, 36 minutes, 15 secondsIt already automatically finds the different filters I wanna use and so on and so forth. And then it's gonna dump these into a little folder for me.
2:36:222 hours, 36 minutes, 22 secondsWhat it'll do after, according to my skill spec, is it's going to read through each of these 25 leads, sometimes do a little bit of background research to say,
2:36:302 hours, 36 minutes, 30 secondshey, are these the sorts of leads that I'm actually, Nick is probably actually interested in?
2:36:342 hours, 36 minutes, 34 secondsAnd then if so, then it proceeds with a full parallel scrape of 1,000 simultaneously, and that occurs quite quickly.
2:36:412 hours, 36 minutes, 41 secondsSo in this case, it started four of these scrapers and it's just parallelizing these, so I'm gonna get 250 from each.
2:36:462 hours, 36 minutes, 46 secondsTo be clear, previously, this probably would have taken me 15 or 20 minutes to set up the filters, to set everything, kind of configure that initial search, right?
2:36:542 hours, 36 minutes, 54 secondsAnd then if I wanted to do that search at 25, I would have had to manually verify them myself, which took me another 10 or 15 minutes.
2:37:002 hours, 37 minutesAfter that, I would have started the actual scraper, then I would have had to upload them into a Google Sheet, I would have had to cross-reference the leads to make sure they're good,
2:37:082 hours, 37 minutes, 8 secondsI would have had to run some additional AI-based flows, and it just would have been a big pain in my ass to make a long story short.
2:37:132 hours, 37 minutes, 13 secondsNow, A is capable of doing this for me in just a couple of minutes.
2:37:162 hours, 37 minutes, 16 secondsAnd I'm running this in terminal because I have access to what's called fast mode right now.
2:37:202 hours, 37 minutes, 20 secondsEssentially, Anthropec's new Opus 4.6 model has launched with the ability to run two and a half times faster for approximately three times the price, so I'm happy to pay a little bit more money
2:37:282 hours, 37 minutes, 28 secondsif it means that I can do all of the knowledge work that I need to do a little bit faster.
2:37:312 hours, 37 minutes, 31 secondsAs you see here on the right-hand side, it's now finding a bunch of my leads for me, it's compiling them into a list, 250 leads done from that search, 250 leads done from that search,
2:37:402 hours, 37 minutes, 40 seconds250 leads done from that search, and we just have one more to go.
2:37:422 hours, 37 minutes, 42 secondsNow, what's really cool about skills is it doesn't need to be right every single time, it's not like a program, it's not like I put something together and then the second that it makes a mistake, it's done.
2:37:502 hours, 37 minutes, 50 secondsAs you see here, it scraped about 1,000 leads in 87 seconds and now it's uploading to Google Sheets. And somewhere along the line, there was an issue.
2:37:572 hours, 37 minutes, 57 secondsAnd the issue was, it turns out I can't use spaces and stuff like that in the file.
2:38:012 hours, 38 minutes, 1 secondSo what it did is it realized that it made a problem, and then it uploaded it to Google Sheets with the proposed solution, it went through and it read through a little bit of the API documentation and stuff like that to do that.
2:38:112 hours, 38 minutes, 11 secondsThis is stuff that I previously would have had to do and that try and retry loop just takes forever.
2:38:152 hours, 38 minutes, 15 secondsOn top of that, what it does is it goes and enriches the emails for me.
2:38:182 hours, 38 minutes, 18 secondsAnd then what I end up with is I end up with a list that looks something like this. So I just bold this and then I make this a little bit bigger.
2:38:232 hours, 38 minutes, 23 secondsI've since hidden the email columns here just because I don't wanna show too much information.
2:38:282 hours, 38 minutes, 28 secondsWe have clinic phone numbers and stuff like that, company phone numbers and addresses.
2:38:312 hours, 38 minutes, 31 secondsBut as you can see here, we have tons of information about dentists that are across the United States.
2:38:362 hours, 38 minutes, 36 secondsLooks like a big chunk of them are in Philadelphia, New York, Moorstown, Boston, we have cities, we have everything that we need.
2:38:422 hours, 38 minutes, 42 secondsAnd so what I'd do with this now is I would take this, then I would send it into a tool like Instantly, which is my cold email platform and then I would immediately start sending.
2:38:492 hours, 38 minutes, 49 secondsAnd as I mentioned, this takes a pre-existing process that would have taken me at least half an hour, probably more, and it turns into one that I literally did in 87 seconds.
2:38:562 hours, 38 minutes, 56 secondsSo as you can see, skills can be extremely economically valuable.
2:38:582 hours, 38 minutes, 58 secondsThe question is, how do you actually go about creating them and creating them in a way that I think is reproducible and efficient and so on and so forth.
2:39:052 hours, 39 minutes, 5 secondsWell, the first thing is you need to know how script or rather skill structure works.
2:39:092 hours, 39 minutes, 9 secondsIf I just zoom in on this to make it a lot easier, you could see that our scrape leads skill is broken up into a few components.
2:39:172 hours, 39 minutes, 17 secondsFirst, we have the folder itself, scrape-leads. Then we have another folder inside called scripts. This runs the program aspect of the skill.
2:39:262 hours, 39 minutes, 26 secondsAnd then finally, we have the actual skill.md in Markdown.
2:39:292 hours, 39 minutes, 29 secondsSo I want you to treat what we're seeing here in the skill.md as basically like the orchestrator of this whole affair.
2:39:392 hours, 39 minutes, 39 secondsSo the skill.md is like the checklist or orchestrator.
2:39:492 hours, 39 minutes, 49 secondsYou know, in an orchestra, the orchestrator is the person with the little, I don't know, those sticks.
2:39:542 hours, 39 minutes, 54 secondsMy sister does some of that actually, which is funny, I don't even know what the hell they're called.
2:39:572 hours, 39 minutes, 57 secondsBut you know, it's where you kind of wave them around and do all that stuff.
2:39:592 hours, 39 minutes, 59 secondsAnd then what happens is, you know, the orchestrator is not the person making, you know, the conductor, I should say, is not the person that's making the music.
2:40:062 hours, 40 minutes, 6 secondsWhat they're doing is they are orchestrating the production of music from a variety of other sources.
2:40:122 hours, 40 minutes, 12 secondsInside of scripts, we have, you know, the actual musicians themselves, violinists,
2:40:202 hours, 40 minutes, 20 secondsyou know, the cellists, we have the pianists, and so on and so on and so forth.
2:40:252 hours, 40 minutes, 25 secondsAnd so basically what occurs is we give it a big checklist of tasks in the skill.md.
2:40:312 hours, 40 minutes, 31 secondsWe give it a bunch of reference information and everything that it needs. And we treat it just like we treat a junior employee. We say, okay, here's the checklist, go out and do it.
2:40:382 hours, 40 minutes, 38 secondsAnd then where the orchestration kind of comes in is if there's an issue with the step-by-step execution of different subtasks, some of which are going to be scripts and stuff like that,
2:40:472 hours, 40 minutes, 47 secondsthen Claude gets to use its own native intelligence to fix it in real time.
2:40:512 hours, 40 minutes, 51 secondsNot only do they fix it, but then it also goes in and it updates the skill so that if there's another issue in the future, or if another instance of Claude tries running this, it doesn't run into the same problem.
2:41:002 hours, 41 minutesSo as you can see, they're very, very valuable.
2:41:022 hours, 41 minutes, 2 secondsThey're more or less exactly the same way that like a person would go and do a task.
2:41:052 hours, 41 minutes, 5 secondsNow inside of my scripts folder here, I have, as you can see, a bunch of different actual Python scripts that have been developed for this purpose. Do I know anything that goes on in here?
Chapter 23: The Structure of Skills
2:41:152 hours, 41 minutes, 15 secondsNo, I haven't even looked at this code.
2:41:162 hours, 41 minutes, 16 secondsIt's probably the first time that I'm ever opening up this file.
2:41:182 hours, 41 minutes, 18 secondsWhat I did is I told Claude that I wanted it to go and do things in my checklist and then go create scripts that would do them all for me.
2:41:252 hours, 41 minutes, 25 secondsIt's much better to do this than just tell Claude to do it from fresh and from scratch every time.
2:41:302 hours, 41 minutes, 30 secondsBecause obviously if it's the same thing that you need to do every time, you should turn it into like a defined program, right? Because then it's always gonna execute similarly.
2:41:372 hours, 41 minutes, 37 secondsThat way Claude is not actually doing the executions himself.
2:41:402 hours, 41 minutes, 40 secondsWhat it's doing is it's just using the scripts here, just like it uses tools.
2:41:442 hours, 41 minutes, 44 secondsYou know how it has access to bash and web search and stuff like that? This is the same idea. It's just we're doing it encapsulated in a skill. Okay, so it takes this information.
2:41:522 hours, 41 minutes, 52 secondsIt goes through the skill. It says, okay, step one is test scrape.
2:41:552 hours, 41 minutes, 55 secondsSo I need to run the scrape appify with this query, max items 25, whatever the heck that means. Then it goes, it executes this. Once it's done, it checks the result.
2:42:042 hours, 42 minutes, 4 secondsIf that's the case, then it goes back and then it runs the same scraper except with different parameters.
2:42:102 hours, 42 minutes, 10 secondsAssuming that that's good, then it uses this classify leads LLM script afterwards to tabulate that information.
2:42:162 hours, 42 minutes, 16 secondsAssuming that that's good, it goes into what looks like update sheet to like create a Google sheet and then send it.
2:42:222 hours, 42 minutes, 22 secondsAssuming that that's good, it then goes enriches the missing emails and so on and so forth.
2:42:262 hours, 42 minutes, 26 secondsAnd there's different paths here based off of how many people we wanna scrape. I have a few other skills that I use pretty often as well. This one's called literature research.
2:42:352 hours, 42 minutes, 35 secondsAnd so, if I'm trying to perform research on a task, I will actually say, go perform a lit review
2:42:412 hours, 42 minutes, 41 secondson the recommended daily dose of, let's say vitamin D and IUD for males in their early thirties.
2:42:512 hours, 42 minutes, 51 secondsWhat this will do, okay, in addition to reading the CLAW.MD to get context about this whole thing, is it'll go through and it'll read this literature research skill.
2:42:592 hours, 42 minutes, 59 secondsIf I open this up so that we can all see, the first thing it's gonna do is it's gonna query like this database, which I'm suggesting that it queries.
2:43:062 hours, 43 minutes, 6 secondsSo this database, I think, was called PubMed.
2:43:092 hours, 43 minutes, 9 secondsAfter that, it's going to analyze using this little deep review script.
2:43:132 hours, 43 minutes, 13 secondsAnd you'll notice that if I make this big again, I made some mistakes here, right?
2:43:172 hours, 43 minutes, 17 secondsFor whatever reason, the first query did not actually work fine, but it ended up redoing it over and over and over again until it figured it out.
2:43:242 hours, 43 minutes, 24 secondsAnd so this is the orchestration aspect that I'm talking about.
2:43:262 hours, 43 minutes, 26 secondsYou can give it a checklist, but obviously not everything goes right perfectly.
2:43:292 hours, 43 minutes, 29 secondsBecause not everything goes right perfectly, you need to give it some flexibility in order to do your tasks. And that's what it's doing right now, right?
2:43:352 hours, 43 minutes, 35 secondsIt's gone through and it's gone and created a bunch of literature review-based information for me.
2:43:402 hours, 43 minutes, 40 secondsWhy would I use this versus, let's just say, telling CLAW to go find that information?
2:43:442 hours, 43 minutes, 44 secondsBecause I've already just put in the infrastructure to query specific databases that I really like.
2:43:502 hours, 43 minutes, 50 secondsI've taught it how to run parallel queries so I can do this research in 10th of the time.
2:43:542 hours, 43 minutes, 54 secondsI've taught it to use models that might be a little bit less capable, but might have much longer context windows and so on and so forth.
2:44:002 hours, 44 minutesAnd so this enables you to find a workflow that works really, really well, and then just consolidate it and then do it the same every time. This is why I no longer hire.
2:44:072 hours, 44 minutes, 7 secondsI mean, my businesses collectively still make over $300-something thousand per month right now in profit. That's a lot of money.
2:44:142 hours, 44 minutes, 14 secondsI don't have staff members to do these things for me anymore.
2:44:172 hours, 44 minutes, 17 secondsAnytime I want anything done, I'll just tell CLAW to do it with one of these skills. Because to be honest, it's the exact same thing anyway.
2:44:242 hours, 44 minutes, 24 secondsI would have just hired a contractor to do this sort of literature research. I would have hired a contractor to do my lead scraping.
2:44:302 hours, 44 minutes, 30 secondsWhy do I have to wait around a whole day or two for them now if I could just execute a skill to go do the thing, retrieve me the results, and then, I don't know, maybe feed it into another skill in 100th of the time for like 100th of the cost.
Chapter 24: Building a New Skill
2:44:392 hours, 44 minutes, 39 secondsOkay, so while all of this is occurring in this tab and I'm doing that research, why don't I show you guys how to actually create a skill in practice. To make a skill, it's really straightforward.
2:44:482 hours, 44 minutes, 48 secondsYou basically just give it like a bullet point list of things that you want it to do.
2:44:512 hours, 44 minutes, 51 secondsSo I'm going to say today we're creating a skill and why don't I just use my voice transcript tool? That's way easier.
2:44:572 hours, 44 minutes, 57 secondsThis skill will design websites in a format that I really like using a template that I really like.
2:45:022 hours, 45 minutes, 2 secondsI want the websites designed very similarly every time because I'm going to use them to pitch people.
2:45:062 hours, 45 minutes, 6 secondsIn short, what I'm going to give you is I'm going to give you a bunch of information about a prospect and then I want you to design a website using a specific template. The template I'm going to supply you is this one.
2:45:162 hours, 45 minutes, 16 secondsAnd then what I'm going to do is, you know how earlier we went through godly.website and then we found a template that we really liked?
2:45:222 hours, 45 minutes, 22 secondsWell, I'm just going to scroll through and I'm going to find a template that I really like.
2:45:262 hours, 45 minutes, 26 secondsSo scrolling through, I don't know, I just want this to be a simple website that I could use for, let's just do like, I don't know, dentists hypothetically right now.
2:45:342 hours, 45 minutes, 34 secondsSo I'm going to go over here and then I like this build an Amsterdam one.
2:45:402 hours, 45 minutes, 40 secondsOkay, and then what I'm going to do is I'm just going to, I think that's it honestly, I think I'm just going to screenshot this and they'll just make one pager for Claude. Just going to screenshot it.
2:45:482 hours, 45 minutes, 48 secondsOkay, and then I'm going to go back to my anti-gravity and then paste it.
2:45:512 hours, 45 minutes, 51 secondsI want you to use screenshot functionality to mirror the style of the website.
2:45:562 hours, 45 minutes, 56 secondsI'm also going to paste in some of the HTML so you can use that to create a style guide, et cetera.
2:46:002 hours, 46 minutesYou'll receive as input, like a Google sheet with information about a prospect and then you just create a website that matches.
2:46:082 hours, 46 minutes, 8 secondsFind web images using publicly available sources, make sure it's really pretty and yeah, follow the template as closely as possible. Then I'm going to go on the website.
2:46:172 hours, 46 minutes, 17 secondsI'm going to, let's just make it really wide because sometimes websites are different.
2:46:222 hours, 46 minutes, 22 secondsAnd then what I'm going to do is I think I'm just going to copy all of this. It's really, really long, right? I'm just going to paste it in. So that's going to be huge.
2:46:302 hours, 46 minutes, 30 secondsIt's going to be a lot of stuff to paste, 474 lines. And then, hmm, anything else that we need to do? I don't think so.
2:46:362 hours, 46 minutes, 36 secondsI guess I just need to give it an example of some of the input.
2:46:382 hours, 46 minutes, 38 secondsSo I'm going to go and then find that Google sheet that we just had with a bunch of dentists.
2:46:422 hours, 46 minutes, 42 secondsThen I'm just going to copy all of this information and I'll say example of the data and then I'll paste that in.
2:46:482 hours, 46 minutes, 48 secondsOkay, so I mean, I just fed in a tremendous amount of information here, right? Like this is really, really big.
2:46:522 hours, 46 minutes, 52 secondsBut with our little fast mode, plus some pretty precise instructions, I think we can probably generate a cool skill
2:47:012 hours, 47 minutes, 1 secondin just a couple of minutes that does this sort of thing automatically.
2:47:032 hours, 47 minutes, 3 secondsAnd after this, I'll have a system where I can basically just feed into Google Sheet and then I can generate a beautiful, customized website for a prospect in like two seconds, which has information about them that clearly is customized and so on and so forth.
2:47:142 hours, 47 minutes, 14 secondsAnd then I can just give it to them as sort of a lead magnet or something. That sounds pretty fun. And it's already gone through and it's done some stuff. Now I'm not using plan mode for this, but you absolutely can.
2:47:212 hours, 47 minutes, 21 secondsI just wanted to one shot a skill with this fast mode just so that I could do something while I was waiting for the literature review to finish.
2:47:272 hours, 47 minutes, 27 secondsAs you can see, it's loaded in the skill pattern and structure from the other skills as well as the Claude.md. And now it's just gonna ask me some information.
2:47:352 hours, 47 minutes, 35 secondsSo where should the output of the skill be? A local HTML file? Yeah, let's just use local HTML for now. How will you provide prospect data? We'll just do Google Sheet URL.
2:47:432 hours, 47 minutes, 43 secondsFor images, which approach do you recommend? Yeah, sure, let's do the unsplash API.
2:47:462 hours, 47 minutes, 46 secondsShould the website be a mockup of what their business site could like or a pitch page about your services? Mockup of their site. Cool, that looks great. So that's sort of its plan mode analogy.
2:47:562 hours, 47 minutes, 56 secondsAnd this actually initiated plan mode without me even having to ask basically.
2:47:592 hours, 47 minutes, 59 secondsI just make this a little bigger so you could see the entire chat.
2:48:022 hours, 48 minutes, 2 secondsThis went through and then turned on plan mode like on its own. I didn't even have to ask it to.
2:48:062 hours, 48 minutes, 6 secondsAnd that's what occurs sometimes when you do bypass permissions.
2:48:082 hours, 48 minutes, 8 secondsIt'll just choose to create a plan for a more complicated software build.
2:48:122 hours, 48 minutes, 12 secondsCool, and now I'm going to bypass permissions and we're gonna go.
2:48:142 hours, 48 minutes, 14 secondsWhile that's occurring, just scrolling through this literature review. Looks pretty cool. It gives me a bunch of information. Apparently one to 2000. So that looks pretty fun.
2:48:212 hours, 48 minutes, 21 secondsOkay, and then this looks like the little demo that we put together. This is a pretty basic demo. Not that big of a fan to be honest. So I think we're gonna have to go do some back and forth.
2:48:292 hours, 48 minutes, 29 secondsSo we did build a website in just a few seconds for them, which is kind of neat. Okay, it's now gonna take a screenshot of this page for us.
2:48:352 hours, 48 minutes, 35 secondsAnd as you can see, it's now accumulated like 19 or 20,000 tokens, which is kind of cool. Here's what we got. Full viewport hero.
2:48:422 hours, 48 minutes, 42 secondsOkay, so I'm just gonna say, not a very big fan of the website design. I don't think this matches the website.
2:48:472 hours, 48 minutes, 47 secondsI'd like you to get pixel perfect accuracy by screenshotting the comparison back and forth. Go find some library that allows you to do this as necessary.
2:48:572 hours, 48 minutes, 57 secondsIn terms of Unsplash, how are you currently getting your images? Let's just do that. That's fine.
2:49:012 hours, 49 minutes, 1 secondI don't really want it to go, you know, force me to get an API key or something like that. I'm just gonna have it run. Okay, that's looking a lot better than what we had before. I like this.
2:49:092 hours, 49 minutes, 9 secondsIt looks like it took some photos of areas that were similar to where this place is located.
2:49:142 hours, 49 minutes, 14 secondsAnd as we scroll through, we obviously have the information and the template and stuff. I don't like how a lot of these images are the same. So I'm just gonna say, nice job. I don't like how all these images are the same though.
2:49:232 hours, 49 minutes, 23 secondsGet different images. Looks really clean. We even have like their phone numbers and stuff like that.
2:49:272 hours, 49 minutes, 27 secondsSo we're now capable of basically like one-shotting a website for somebody.
2:49:302 hours, 49 minutes, 30 secondsAnd as you can see, we can generate these super, super easily and very quickly.
2:49:352 hours, 49 minutes, 35 secondsWhat I'll do now that we have this is realistically, I'm gonna try it with one row and then just see how quickly it can put together the site for me. Cool.
2:49:412 hours, 49 minutes, 41 secondsNow I'm going to test it with some new information and let's see how quickly it can put that together.
2:49:462 hours, 49 minutes, 46 secondsCool. We've now done the same thing with Bennington Dental Center. So that's neat. We have some images generated and stuff like that.
2:49:522 hours, 49 minutes, 52 secondsIt is telling me that the reason why we have images of dogs and stuff is because I don't want to supply my Unsplash API key.
2:49:582 hours, 49 minutes, 58 secondsYou know, if we do, then obviously there'll be much more dentally oriented. I think that's fine. Hopefully you guys get the idea. You could build stuff like this really quickly. In this case, it took 30 seconds.
2:50:072 hours, 50 minutes, 7 secondsSo I mean, like what we could do if we wanted to like build this out as a service and like actually just like generate custom websites for people, send them out and so on and so forth.
2:50:142 hours, 50 minutes, 14 secondsWe could turn the skill into a sub agent to show you guys how to do later where basically we can spin up 10 of these simultaneously and basically in parallel, just generate 10 every 30 seconds.
2:50:232 hours, 50 minutes, 23 secondsThat's a per website generation time of about three seconds.
2:50:262 hours, 50 minutes, 26 secondsAnd so now that we're generating them every three seconds with customized information, matching widths and heights and stuff like that, making it really custom and sexy, you know, you could do 10,000 leads in approximately 30,000 seconds.
2:50:362 hours, 50 minutes, 36 secondsI don't know how long that would actually take. Let's see. 30,000 divided by 60 is 500.
2:50:412 hours, 50 minutes, 41 secondsSo it might take 500 minutes or I don't know, eight hours for a full 10,000 list.
2:50:462 hours, 50 minutes, 46 secondsBut yeah, combine that with the scraper, combine that with you sending people customized websites and combine that with some other skills that I've set up to like automate the process of whipping up instantly campaigns and stuff.
2:50:542 hours, 50 minutes, 54 secondsAnd hopefully you guys can see we get a pretty solid system in our hands. And that took me just a few minutes to put together.
Chapter 25: Introduction to Model Context Protocol
2:50:582 hours, 50 minutes, 58 secondsNow that we know all about skills, let's talk a little bit about the next logical thing, which is model context protocol.
2:51:042 hours, 51 minutes, 4 secondsSo now that you guys understand sort of how skills work, which to be clear is that skills are basically like backend functions that you can run scripts almost
2:51:132 hours, 51 minutes, 13 secondsthat use the flexible intelligence of AI and the procedural rigor of Python scripts and other programming tools to do the same thing every time, but also allow you the flexibility to air handle.
2:51:262 hours, 51 minutes, 26 secondsIt's not that we understand that. Let's talk about model context protocol.
2:51:302 hours, 51 minutes, 30 secondsAnd so the way that I see it is these are just skills except other people make the skills for you.
2:51:352 hours, 51 minutes, 35 secondsI want to say other people, for the most part, it's like developer teams and stuff like that. Very similar idea.
2:51:402 hours, 51 minutes, 40 secondsYou're basically just giving your agent access to a piece of software.
2:51:442 hours, 51 minutes, 44 secondsAnd then just like it calls its own tools like web search and bash and the Chrome dev tools, MCP and the screenshot, all the stuff that we've already looked at.
2:51:512 hours, 51 minutes, 51 secondsWhat we're doing here is we're just calling them, but somebody else is responsible for putting them together.
2:51:582 hours, 51 minutes, 58 secondsNow that obviously begs the question, where do I get MCPs?
2:52:032 hours, 52 minutes, 3 secondsWell, you can just go on websites like MCP servers.org, model context protocol slash servers, and then MCP market.
2:52:112 hours, 52 minutes, 11 secondsI want you guys to know that not all of these are going to be 100% safe or secure.
2:52:152 hours, 52 minutes, 15 secondsThese are third party libraries that people are putting together basically that try and tabulate the number one MCP skills and so on, MCP tools and so on and so forth.
2:52:252 hours, 52 minutes, 25 secondsBut a lot of these are pretty well vetted at this point.
2:52:272 hours, 52 minutes, 27 secondsAnd I'm going to show you at least a couple that I really like. The biggest one is probably the Chrome dev tools MCP.
2:52:332 hours, 52 minutes, 33 secondsThis is one that I use constantly, basically every day, many, many times, because it allows your coding agent to control and then inspect a live Chrome browser.
2:52:412 hours, 52 minutes, 41 secondsIn my opinion, it is significantly higher quality than any of the current browser tools that, Clawed or other platforms have given us.
2:52:492 hours, 52 minutes, 49 secondsSo I mean, I have this little Chrome extension here that I can actually use to control this instance of Chrome through this Clawed tool.
2:52:562 hours, 52 minutes, 56 secondsIt's developed specifically by the Clawed team. And I could say, hey, summarize this page.
2:53:012 hours, 53 minutes, 1 secondAnd then what it can do is it can copy all the text on this page, so it can extract the page text. And then it can take a screenshot of it.
2:53:092 hours, 53 minutes, 9 secondsAnd then it can tell me about it and so on and so forth. I can also have it do things like click.
2:53:142 hours, 53 minutes, 14 secondsI could say, okay, star this on GitHub or something like that.
2:53:212 hours, 53 minutes, 21 secondsAnd then I can give it some additional instructions and then it can go through, look for the GitHub link. I don't know, maybe click it.
2:53:282 hours, 53 minutes, 28 secondsAnd then now that we have this thing open, it's gonna go and it's gonna try and star this puppy. So that's pretty cool. But you'll find that it also takes a fair amount of time.
2:53:362 hours, 53 minutes, 36 secondsAnd the Chrome DevTools MCP bypasses that completely. And it's like a hundred times faster.
2:53:402 hours, 53 minutes, 40 secondsAnd not only is it a hundred times faster, because you can weave it into skill-based scenarios, you can actually just run really, really procedural things
2:53:472 hours, 53 minutes, 47 secondsin the browser that previously would have taken you like a fair amount of time to automate.
2:53:512 hours, 53 minutes, 51 secondsSo I mean, I already have access to this right now, but for the purpose of this demonstration, let me just open up a new Clawed instance in my terminal and then double check the fast mode is on.
2:53:592 hours, 53 minutes, 59 secondsIt looks like it is.
2:54:012 hours, 54 minutes, 1 secondAll I would really do if I wanted to download the MCP for any tool really now is I would just paste in this definition you can find right over here.
2:54:122 hours, 54 minutes, 12 secondsBasically all MCP tools are going to have some sort of JSON that looks like this, where there's a curly bracket, it says MCP servers, it'll say the server name.
2:54:192 hours, 54 minutes, 19 secondsThere's a bunch of commands in RX, which really don't matter whatsoever.
2:54:222 hours, 54 minutes, 22 secondsBut basically you just go on whatever page of the MCP supplies this information. You copy this in.
2:54:282 hours, 54 minutes, 28 secondsSo if you want to install one, all you really do is you just paste in that little, you know, JSON snippet that we saw earlier. And we say, I want to install this in my local workspace.
2:54:392 hours, 54 minutes, 39 secondsIt's important that you say local workspace here.
2:54:422 hours, 54 minutes, 42 secondsWhat it's going to do is just going to grab that data and then install it for you. My case, it's already installed.
2:54:462 hours, 54 minutes, 46 secondsSo I don't actually need to change anything, but now I'll say, great, run it.
2:54:512 hours, 54 minutes, 51 secondsNow, sometimes when you're using a tool that requires authentication, what it'll do is it'll force you to go and grab an API key or an API token or something like that.
2:55:002 hours, 55 minutesSo I'm going to show you guys an example of using a tool that requires some API credentials in a second.
2:55:052 hours, 55 minutes, 5 secondsFirst, let's just say, okay, open and navigate to left click AI, then screenshot the site and tell me about it visually.
2:55:152 hours, 55 minutes, 15 secondsSo what it's going to do is it's going to open up that browser, going to navigate to left click.ai.
2:55:192 hours, 55 minutes, 19 secondsNow it's going to take a screenshot, which I think it just did.
2:55:222 hours, 55 minutes, 22 secondsNow it's going to read the screenshot and then it's going to, I don't know, give me some high level stuff about the website.
2:55:272 hours, 55 minutes, 27 secondsSo the header is a simple nav bar with the left click logo, case studies about links and a let's talk CTA button, hero sections, large, bold, serif, right?
2:55:362 hours, 55 minutes, 36 secondsOkay, great.
2:55:382 hours, 55 minutes, 38 secondsGo to amazon.ca and find me a bunch of cheap, but effective light boxes for my studio. So now I'm going to open this up.
2:55:472 hours, 55 minutes, 47 secondsIt's going to do the same thing with Amazon.
2:55:482 hours, 55 minutes, 48 secondsI'm in Canada, hence the .ca, and it's going to start pumping in various search terms for light boxes and whatnot.
2:55:562 hours, 55 minutes, 56 secondsI don't know why Twilight is recommended to me. Must say something about my browsing history.
2:56:022 hours, 56 minutes, 2 secondsNow it's going through, finding a bunch of light boxes and stuff like that.
2:56:052 hours, 56 minutes, 5 secondsIt's going to take screenshots of the page and then deliver me a bunch of options that I could choose from. And you can see all this occur underneath the tool call.
2:56:132 hours, 56 minutes, 13 secondsSo this little green boxes are, little green circles, I should say, are tool calls. They have the specific name of the MCP over here.
2:56:212 hours, 56 minutes, 21 secondsAnd then they have the tool that they're calling from the MCP over here as well.
2:56:242 hours, 56 minutes, 24 secondsAnd what they're doing here is now that it's taking a screenshot and stuff like that, it's giving me summaries of all the information. Then it's even recommending a certain one.
2:56:312 hours, 56 minutes, 31 secondsNow, a lot of the time, you can also just type in the name of the tool you want, and then the word MCP server.
2:56:372 hours, 56 minutes, 37 secondsAnd a lot of these tools will actually have gone through and then created the stuff.
2:56:402 hours, 56 minutes, 40 secondsThat'll take me to the clickup page with MCP server setup instructions.
2:56:432 hours, 56 minutes, 43 secondsAnd then what I'm going to do is I'm just going to copy over this stuff, just like I did before. And then we're going to go back to my agent. So what I'm going to do is I'll just go install this MCP.
2:56:512 hours, 56 minutes, 51 secondsI'm going to paste this in. This includes all of the details and documentation here.
2:56:562 hours, 56 minutes, 56 secondsSo first thing it's going to do is look for some sort of configuration file that's preexisting. It's not going to find one. So it's going to go and then just make mine.
2:57:022 hours, 57 minutes, 2 secondsThen eventually what it's going to ask me to do is go grab my API key. So I'll head over here to click up API. And then I'm just going to copy my API token. I'll then confirm it.
2:57:102 hours, 57 minutes, 10 secondsAnd then I can copy my token in, and then I'll go back here. Then I'll say, great. Here's my API details. Going to feed that in.
2:57:192 hours, 57 minutes, 19 secondsAnd then every time you install a new MCP server you do have to open up a new thing. So that's what I just did here. Next thing it asked me which workspace I want to connect to. I have multiple, so I'll click connect workspace here.
2:57:282 hours, 57 minutes, 28 secondsThen I can just go back and then I'll say, great. Do you have access to my ClickUp MCP?
2:57:382 hours, 57 minutes, 38 secondsI'll say, awesome. Create a new content idea called Claude Code course.
2:57:452 hours, 57 minutes, 45 secondsNow what it's going to do is scroll through all of my lists.
2:57:482 hours, 57 minutes, 48 secondsIt's then going to search for various ones that I may or may not have.
2:57:522 hours, 57 minutes, 52 secondsSo I have lists called like content ideas and trends and stuff like that. And I'm going to ask it to insert it in my main YouTube queue. So I'm going to do that.
2:57:592 hours, 57 minutes, 59 secondsAnd now you can see there's a task called Claude Code course. If I go back here to my ClickUp, open up the specific task.
2:58:072 hours, 58 minutes, 7 secondsThere's now a course that's basically been created. I don't like how the status is archived. So I'm going to say, the status is archived right now. And as you see here, it's now set to record.
Chapter 26: Evaluating Token Usage in MCPs
2:58:152 hours, 58 minutes, 15 secondsSo that's pretty neat.
2:58:162 hours, 58 minutes, 16 secondsThe last thing I want to talk about now is if we go slash context and scroll all the way up, you start to get an appreciation for just how many tokens can get used up by poorly drawn or poorly written MCPs.
2:58:292 hours, 58 minutes, 29 secondsAnd so in this case, I'm not saying the ClickUp MCP is really that bad. It's not terrible.
2:58:322 hours, 58 minutes, 32 secondsI've seen many, many far worse ones, but it does consume a hell of a lot of tokens. As you see here, just the ClickUp search tool consumes 1,600.
2:58:402 hours, 58 minutes, 40 secondsThe Get Workspace hierarchy is 419. This one's 1.1K.
2:58:442 hours, 58 minutes, 44 secondsIf we added all of them up together, as you could see, my MCP tooling is now taking up almost 20,000 tokens.
2:58:512 hours, 58 minutes, 51 secondsThat's actually now more than the system tools, which previously used to be really, really big.
2:58:562 hours, 58 minutes, 56 secondsAnd what that means is right off the very get-go, basically like right at the very beginning, we are already at something like 35,000 tokens or so before I enter my prompt, before I enter anything.
2:59:082 hours, 59 minutes, 8 secondsIf you take into account the system prompt as well, we're now at 40,000. You take into account some memory files and my skills. We're not closer to 45.
2:59:162 hours, 59 minutes, 16 secondsAnd this is all before I've sent a message, right?
2:59:172 hours, 59 minutes, 17 secondsKeep in mind that 45% is, I don't know, let's just say 45 over 200. That's about equivalent to a quarter.
2:59:262 hours, 59 minutes, 26 secondsAnd so one quarter of all my contexts, and by the way, this is the highest quality section of my prompt.
2:59:332 hours, 59 minutes, 33 secondsIf I were to write actual messages here, this would be the highest quality output.
2:59:372 hours, 59 minutes, 37 secondsBasically, the highest ROI section is currently being taken up by a ton of MCP tools and stuff.
2:59:422 hours, 59 minutes, 42 secondsIn addition, you'll compare this to skills, and you'll see that scrape leads only takes up 63 tokens. School monitor takes up 59, right?
2:59:502 hours, 59 minutes, 50 secondsCross-niche outliers takes up 58.
2:59:522 hours, 59 minutes, 52 secondsSo it's like, oh wow, a single one of these MCP tools, like update task, consumes more than basically all of my skills combined.
3:00:003 hoursIt's kind of like, why the hell would I even use MCP tooling if I can just do a skill instead? The reason really is just the convenience of it.
3:00:063 hours, 6 secondsMCP, as you see, is pretty easy to set up where skills, as you saw, take a little bit longer.
3:00:103 hours, 10 secondsThat skill back there that does those website designs, that took me, I don't know, probably like five minutes end to end to create.
3:00:153 hours, 15 secondsOnce I've created it, it's obviously super efficient and so on and so forth.
3:00:193 hours, 19 secondsBut the click of MCP, all I really had to do was just like log in and then give it one line, and then I did that.
3:00:243 hours, 24 secondsSo basically the way that I personally use MCPs is I use them aside from the Chrome DevTools MCP, because I just think that's fire, and I use it all the time.
3:00:303 hours, 30 secondsI use them to very quickly sketch out whether or not something's possible.
3:00:333 hours, 33 secondsI'll basically go to a new tool that I wanna see if I can integrate, and I'll just say, hey, here's the MCP details, can we do X, Y, and Z?
3:00:393 hours, 39 secondsAnd then if it can do X, Y, Z the first time, then I'll say, okay, this is great.
3:00:413 hours, 41 secondsI want you to take what you just did, and I want you to convert it to a skill instead.
3:00:453 hours, 45 secondsAnd I want you to go and find like the API endpoints and stuff, and then build a script that does all that for me instead of me having to use the super bloated MCP tool.
3:00:533 hours, 53 secondsBy the way, have you ever wondered why skills consume so few tokens relative to everything else?
3:00:573 hours, 57 secondsThat's because the whole skill is not actually loaded into context. We go to the scrape lead skill.
3:01:033 hours, 1 minute, 3 secondsThe only section here that's actually loaded into context is this section right up here.
3:01:073 hours, 1 minute, 7 secondsAnd this in markdown format is referred to as the front matter of the file.
3:01:133 hours, 1 minute, 13 secondsAnd so what's really cool is the Claude code developers realized that they could load in a name and description and then some allowed tools to the front matter, and then only feed that into context.
3:01:253 hours, 1 minute, 25 secondsAnd then only if Claude really thinks that it needs to use us if I specifically say, hey, use the scrape leads file, then and only then will it actually load it all.
3:01:313 hours, 1 minute, 31 secondsWhich means I get most of the benefits of having access to a bunch of tools and giving my agent the ability to do so much of things, but I don't have to like load all that in the context immediately.
3:01:403 hours, 1 minute, 40 secondsWhich means I get better decision making at the beginning because performance in prompts are typically the best the very beginning of said prompt of the context window I should say.
3:01:483 hours, 1 minute, 48 secondsAnd then I also don't have to pay a lot of money for it.
3:01:513 hours, 1 minute, 51 secondsSo just another point towards Anthropic, minimizing our total costs, which I think I very much appreciate.
3:01:553 hours, 1 minute, 55 secondsSo just because this is a practical course, I'm actually gonna show an example of this. I'm gonna draft out a task.
3:02:003 hours, 2 minutesI'm then gonna try it with an MCP server, which is gonna be a 10th of the time to implement.
3:02:033 hours, 2 minutes, 3 secondsAnd then if it works, I'm gonna build a skill to do it instead.
3:02:063 hours, 2 minutes, 6 secondsWhat I wanna do today, sort of like my task is I want to label my emails.
3:02:133 hours, 2 minutes, 13 secondsSo what I'm gonna do, if you think about it, the task is really I'm going to list last, I don't know, X emails.
3:02:223 hours, 2 minutes, 22 secondsI'm gonna have Claude read them.
3:02:253 hours, 2 minutes, 25 secondsAnd then I'll also have it label according to some scheme that I put together.
3:02:343 hours, 2 minutes, 34 secondsAnd in this way, I'm not gonna replace the job of an email manager, but I'm gonna make the job of an email manager much easier.
3:02:393 hours, 2 minutes, 39 secondsAnd if later on I want Claude to, how do I manage my emails or whatever, well, now it'll have some pre-existing labels and organized structures for it. So first thing I'm gonna do is I'm gonna go back to Claude.
3:02:483 hours, 2 minutes, 48 secondsLet's go to anti-gravity.
3:02:493 hours, 2 minutes, 49 secondsI'm gonna do this in the GUI this time, not the other mechanism, not the terminal, because I don't think fast mode's super important for this.
3:02:573 hours, 2 minutes, 57 secondsAnd I'm just gonna say, hey, I want to organize my personal mailbox.
3:03:003 hours, 3 minutesCould you provide me a list of high ROI labels that tend to work well for personal mailboxes?
3:03:063 hours, 3 minutes, 6 secondsJust make sure the thinking tab is on, because I want it to really think hard.
3:03:093 hours, 3 minutes, 9 secondsAnd then what I'm gonna do is I'm gonna go to one of my personal mailboxes, and then I'm going to basically implement this. I really like action-based, that sounds great. Keep that for now.
3:03:173 hours, 3 minutes, 17 secondsAnd then I'm gonna open up one of my mailboxes here. We have tons of different emails.
3:03:223 hours, 3 minutes, 22 secondsMost of these are spam, to be honest, or little demos that I put together for whatever purpose.
3:03:273 hours, 3 minutes, 27 secondsI'm also part of what looks to be like a Slack workspace for one of my businesses.
3:03:313 hours, 3 minutes, 31 secondsI think I just did that, because I wanted to test what this looked like.
3:03:333 hours, 3 minutes, 33 secondsNow that that's done, what I'm gonna do is I'm gonna see how I can implement the Gmail MCP really quickly. Great, this looks solid. How do I use the Gmail MCP?
3:03:413 hours, 3 minutes, 41 secondsIt's now going to go and search for Gmail MCP, first in my folders. And then it'll ask me what it wants me to do.
3:03:493 hours, 3 minutes, 49 secondsI want to set one up, so it's gonna start doing some searches for Gmail MCP servers. I'm also gonna do some searching myself.
3:03:563 hours, 3 minutes, 56 secondsThere are probably three or four different ones that realistically work. This looks to be a pretty interesting repo. So what I could do is I could just use this puppy. That looks nice. I just paste this in.
3:04:043 hours, 4 minutes, 4 secondsIt looks like I actually beat Claude to something for once. Now it's gonna compare. Great, let's do it.
3:04:123 hours, 4 minutes, 12 secondsIt's a personal Gmail. Is that okay? Okay, it's now gonna walk me through. So I'm gonna give this button a click. Okay, I went through and I got that data. I'm now gonna paste this in.
3:04:213 hours, 4 minutes, 21 secondsIt's gonna go find the credentials file that I just uploaded. Now it can do what it needs to do. I just need to restart the Claude code session. So I'm just going to open up a new one here.
3:04:313 hours, 4 minutes, 31 secondsThen I'll go back and then I'll say, hey, I want you to label my emails. Ooh, you know, and I don't actually remember. What was that scheme that it asked?
3:04:403 hours, 4 minutes, 40 secondsHey, I want you to label my emails according to this scheme. So now it's going to call the Gmail MCP. Okay, it's going to check the Gmail MCP tools.
3:04:483 hours, 4 minutes, 48 secondsJust figured it out.
3:04:493 hours, 4 minutes, 49 secondsWe have a bunch of pre-existing labels, so it's just gonna create a bunch on its own.
3:04:523 hours, 4 minutes, 52 secondsThe reason why this is occurring so quickly is because I'm using their fast mode. So it's about two and a half times faster than usual. It's now reading through a bunch of emails.
3:04:593 hours, 4 minutes, 59 secondsAnd now it's going to, in addition to thinking through them, go through and then do said labeling.
3:05:043 hours, 5 minutes, 4 secondsAnd then, you know, it's just gonna continue doing this for as many emails as I say.
3:05:083 hours, 5 minutes, 8 secondsSo I think I said that I was gonna do, I don't know, 15 emails or something like that. It's just a 10. So it's 15 inbox emails. Looks great.
3:05:163 hours, 5 minutes, 16 secondsWhy don't we do this for 100 emails in total?
3:05:203 hours, 5 minutes, 20 secondsNow, if I go back into my email, which I think was over here, you can see that I now have different labels. Just gotta refresh that.
3:05:283 hours, 5 minutes, 28 secondsThere's action required or reference and waiting on.
3:05:303 hours, 5 minutes, 30 secondsSo, you know, if something requires my action, some security alerts and stuff like that, then that's one thing.
3:05:353 hours, 5 minutes, 35 secondsIf it's a reference, so this is just stuff that it's storing that may be useful for me. And then there's waiting on down here.
3:05:403 hours, 5 minutes, 40 secondsSo now that I've demonstrated that I could do this sort of thing pretty quickly with a setup that realistically only took me a few minutes, I wanna turn this into a skill.
3:05:473 hours, 5 minutes, 47 secondsSo I'm actually gonna pause this and I'll say, great, this worked really well.
3:05:513 hours, 5 minutes, 51 secondsI'd like to turn this into a skill called Gmail underscore label.
3:05:543 hours, 5 minutes, 54 secondsBasically what I want you to do is just to call the Gmail API directly and then do all of this labeling for me instead of me having to use MCP
3:06:023 hours, 6 minutes, 2 secondsbecause skills are just a lot more token efficient than MCPs.
3:06:053 hours, 6 minutes, 5 secondsCheck out my other skills so you could see an example of how to format them and so on and so forth.
3:06:093 hours, 6 minutes, 9 secondsAnd then write me a skill that effectively does this as well as uses Gmail scripts. Feed that puppy in and then press enter.
3:06:153 hours, 6 minutes, 15 secondsNow I have some other skills that, you know, might have something to do with Gmail.
3:06:183 hours, 6 minutes, 18 secondsSo if it finds them, then it'll probably just wanna use those.
3:06:213 hours, 6 minutes, 21 secondsOkay, cool, it looks like it's rebuilding at all, which is fantastic. We're gonna do the Gmail label skill directory. So it's gonna pump in somewhere right around here.
3:06:303 hours, 6 minutes, 30 secondsLooks like I'm running into some error here. So we're gonna have to do some debugging. Just gonna paste this in directly. Okay, and it looks like we're just about to wrap this up.
3:06:373 hours, 6 minutes, 37 secondsSo now I'm gonna select, say that it can see, edit my email labels and so on and so forth. Now that it's done, the authentication flow has completed.
3:06:453 hours, 6 minutes, 45 secondsI may close this window.
3:06:463 hours, 6 minutes, 46 secondsGoing back over here now, it is created with full Gmail sheets and drive access, which allow me to do this much faster. So you guys seeing just how much quicker this is?
3:06:543 hours, 6 minutes, 54 seconds100 emails immediately fetched.
3:06:563 hours, 6 minutes, 56 secondsIt's now reading and classifying all of them using direct API calls instead of MCP server tools.
3:07:003 hours, 7 minutesAnd then in addition, you know, as I showed you guys earlier, we go to Gmail label. The only thing that's currently being loaded is this.
3:07:083 hours, 7 minutes, 8 secondsAnd this is so much shorter than like the whole MCP skill stuff. So it fetched all 100. It's now categorizing them into five and 95.
3:07:153 hours, 7 minutes, 15 secondsSo it looks like zero is waiting on 95 for reference and then five for action required. That was way faster than what we were doing previously, right? That would have taken probably like five to 10X the time.
3:07:233 hours, 7 minutes, 23 secondsLooks great, why don't we do another 100 and then time yourself. So tell me how long it took. Okay, it's now gonna grab all of these.
3:07:293 hours, 7 minutes, 29 secondsSo it's just gonna continue the filtering process by using some Gmail stuff. Then it's also gonna add some timing instrumentation. That's kind of cool, just cause I'm curious.
3:07:383 hours, 7 minutes, 38 secondsFetch was one second.
3:07:393 hours, 7 minutes, 39 secondsSo we fetched 100 emails in one second compared to previously where it took significantly longer cause I think the MCP tooling had like some built-in thing.
3:07:453 hours, 7 minutes, 45 secondsCool, the end result was it was 36 seconds to fetch, classify and then label 100 emails, about 3.6 seconds per email.
3:07:523 hours, 7 minutes, 52 secondsSorry, 0.36 seconds per email if you think about it that way.
3:07:553 hours, 7 minutes, 55 secondsThen it also gave me some breakdowns and stuff like that of what it is.
3:07:583 hours, 7 minutes, 58 secondsI could run this across like my several thousand outstanding emails if I wanted to.
3:08:023 hours, 8 minutes, 2 secondsI could also do things like have it automatically generate replies to each email.
3:08:073 hours, 8 minutes, 7 secondsWe could build a sub agent, which I'll show you guys how to do stuff like that in a moment where we split each into a parallel tasks and so on and so forth. So guys really the limit here.
3:08:153 hours, 8 minutes, 15 secondsOkay, next up I wanna chat a tiny bit about Claude Code plugins.
3:08:183 hours, 8 minutes, 18 secondsI personally don't use plugins a ton, but they are out there.
3:08:213 hours, 8 minutes, 21 secondsAnd so it's fair if I'm building a masterclass course all about Claude Code, might as well know what the heck these are.
3:08:273 hours, 8 minutes, 27 secondsSimplest and easiest way to access plugins is just go customize and manage plugins.
3:08:313 hours, 8 minutes, 31 secondsIt'll show you the plugins that you currently have installed to see the only one that I have installed so far is called a cloud-mem at the .mac.
3:08:373 hours, 8 minutes, 37 secondsThis is basically a simple, straightforward plugin that basically just adds all of the messages that send any cloud instance to some memory file.
3:08:453 hours, 8 minutes, 45 secondsAnd then cloud can run searches over it if I say, hey, what did I ask you about two weeks ago? So, you know, it's marginally useful.
3:08:513 hours, 8 minutes, 51 secondsYou then have access directly from cloud to a bunch of other ones that are somewhat useful.
3:08:553 hours, 8 minutes, 55 secondsThey have like front-end design, for instance, which is kind of cool.
3:08:583 hours, 8 minutes, 58 secondsSo this is Anthropics own library, which improves the quality, at least they say it improves the quality of a front-end work.
3:09:053 hours, 9 minutes, 5 secondsYou can build sexier and cleaner designs and stuff like that. You know, I don't know, it's kind of 50-50.
Chapter 27: Exploring Claude Code Plugins
3:09:103 hours, 9 minutes, 10 secondsThey say like, if you're doing stuff without the aesthetics prompt, it looks like this.
3:09:133 hours, 9 minutes, 13 secondsAnd then if you're doing it with the aesthetics prompt, it looks like that. Personally, I think both of these are pretty bad.
3:09:173 hours, 9 minutes, 17 secondsThis one definitely looks better, of course, but it's not like that much better. Same thing over here.
3:09:213 hours, 9 minutes, 21 secondsSo I don't know if the guys that made this just weren't like actually crazy front-end devs or anything like that.
3:09:263 hours, 9 minutes, 26 secondsBut I personally think my workflow just going to one of these websites and then copying the screenshot over and then moving everything into Claude Code is like way higher quality.
3:09:343 hours, 9 minutes, 34 secondsBut there are other cool ones here. Context 7 is pretty nice.
3:09:363 hours, 9 minutes, 36 secondsContext 7 basically just allows you to search through any API doc without really having to like know anything about the API docs themselves.
3:09:443 hours, 9 minutes, 44 secondsYou know, if you're working with like three or four different tools, you just install this as a plugin and then it'll automatically shrink and then compress API documentation from the sources over to cloud.
3:09:543 hours, 9 minutes, 54 secondsAnd then it can read it in a very token-efficient manner and do cool things with. None of these things I want to say are required.
3:09:593 hours, 9 minutes, 59 secondsVanilla Claude Code does really, really well without any sort of extensions or plugins at the moment. But you know, just worth us chatting briefly about that.
3:10:063 hours, 10 minutes, 6 secondsAnd then there are two major marketplaces right now that are sort of, well, sorry, one major marketplace right now that's supported by cloud.
3:10:123 hours, 10 minutes, 12 secondsIt's this Claude Code plugins directory, which isn't the cloud-plugins-official repository.
3:10:183 hours, 10 minutes, 18 secondsYou can find all of the plugins just by going to plugins and you'll see there's a big list of ones that they support right out of the box.
3:10:233 hours, 10 minutes, 23 secondsSo they have Agent SDK Dev, they have Code Review, they have C Sharp LSP example plugin. But then there are also open marketplaces.
3:10:313 hours, 10 minutes, 31 secondsSo if you go to Cloud Plugin Marketplace, you'll see that there are a few other ones that people have put together here.
3:10:373 hours, 10 minutes, 37 secondsSo this, for instance, is the Claude Code Marketplace put together by a third-party resource. I could say Anthropic wants me to take down this website. That's pretty funny.
3:10:463 hours, 10 minutes, 46 secondsWith, you know, like chat GPT prompts, let's see. Superpowers, I don't know exactly what that does. We have the Context 7 again.
3:10:533 hours, 10 minutes, 53 secondsA bunch of Claude Code skills that looks like some other people have put together, although not all of these links are gonna work.
3:10:593 hours, 10 minutes, 59 secondsAnd yeah, you know, the plugin installation process is pretty straightforward, as you guys saw earlier. So I'll leave it at that.
3:11:043 hours, 11 minutes, 4 secondsI think plugins are sort of going to be deprecated and probably just absorbed into skills at some point. So I don't wanna spend forever on them.
Chapter 28: Introduction to Sub-Agents
3:11:103 hours, 11 minutes, 10 secondsOkay, and finally, we have subagents, which I think a lot of people here were waiting for. I want you guys to know that subagents aren't like a cure-all. These things aren't actually that incredible.
3:11:183 hours, 11 minutes, 18 secondsYou can do more or less everything that you could do with subagents as of the time of this recording, just with like a normal master agent.
3:11:243 hours, 11 minutes, 24 secondsBut subagents do speed things up a little bit and then they also allow you to parallelize your workflow, which can be quite useful in specific circumstances.
3:11:323 hours, 11 minutes, 32 secondsOne major issue that people currently have with subagents is they consume a ton of tokens and then in doing so can also cost a fair amount, especially when you go to agent teams, which as of the time of this recording,
3:11:403 hours, 11 minutes, 40 secondsis seven times the token usage of just using like one single Cloud thread like I've been doing throughout this course. But subagents are so pretty useful to know.
3:11:483 hours, 11 minutes, 48 secondsAnd so the very first thing I'm gonna do is I'm just gonna show you through example and then we can actually look more into like the subagent spec and stuff like that.
3:11:533 hours, 11 minutes, 53 secondsSo you know how earlier we built this system, the skill, which fetches, classifies and labels 100 emails with zero failures.
3:11:593 hours, 11 minutes, 59 secondsWhat I'd like to do now is I'd basically like to turn this skill into a subagent.
3:12:033 hours, 12 minutes, 3 secondsSo what I'm gonna do is I will remove this so we're not loading any more stuff into context.
3:12:083 hours, 12 minutes, 8 secondsHey, I'd like you to turn this Gmail-label flow into a subagent.
3:12:133 hours, 12 minutes, 13 secondsThe reason why is because I want you to parallelize your work.
Chapter 29: Transforming Skills into Sub-Agents
3:12:163 hours, 12 minutes, 16 secondsInstead of it taking 36 seconds to fetch, classify and label 100 emails, I want you to be able to spawn 10 subagents that do all of those simultaneously and then return the results.
3:12:263 hours, 12 minutes, 26 secondsI'd like you to do this using the subagent spec.
3:12:283 hours, 12 minutes, 28 secondsIf you don't know what that is, do a little bit of research on subagents.
3:12:313 hours, 12 minutes, 31 secondsIt's an anthropic and Claude Code feature that's quite well supported by our current workspace structure.
3:12:363 hours, 12 minutes, 36 secondsAnd once you've built the subagent using a sonnet-4.5, I want you to roll it out as a test and then show me how much faster it is with some sort of timing instrumentation.
3:12:453 hours, 12 minutes, 45 secondsOkay, so I have all that here. I'm now just gonna feed it into my prompt.
3:12:483 hours, 12 minutes, 48 secondsIf we open up this little thinking tab, it's gonna start by researching the subagents and then building a parallelized Gmail-label flow that spawns multiple subagents to classify emails simultaneously.
3:12:573 hours, 12 minutes, 57 secondsI'm gonna use sonnet-4.5 because it can load much more into context, probably read all my emails, and then it can actually go through this whole process and then essentially parallelize it
3:13:053 hours, 13 minutes, 5 secondsand significantly improve the probability and speed that these things are working well and fast.
3:13:103 hours, 13 minutes, 10 secondsThe very first thing it's gonna do is actually spin up a bunch of subagents to do research. So that's what this task little bubble is, right?
3:13:153 hours, 13 minutes, 15 secondsWhen it says research Claude Code subagents, what it's actually doing is it's giving this task to a subagent called the research subagent.
3:13:213 hours, 13 minutes, 21 secondsThere's another subagent as well, like the search subagent.
3:13:243 hours, 13 minutes, 24 secondsSo it'll actually search through my workspace to see if there are any pre-existing subagent patterns.
3:13:273 hours, 13 minutes, 27 secondsAnd then because it's capable of spawning these simultaneously, typically retrieves the results much faster than normal. So that's kind of fun.
3:13:343 hours, 13 minutes, 34 secondsIt's a little bit meta of Claude Code to do that without really understanding what subagents are out of the box.
3:13:383 hours, 13 minutes, 38 secondsOkay, it's now going to create a subagent directory inside of my .cloud folder, and then it's gonna populate it with all the subagent spec parts and everything else.
3:13:473 hours, 13 minutes, 47 secondsAnd what's really cool is we're using subagents alongside skills in this instance. And that's what I'd usually recommend.
3:13:533 hours, 13 minutes, 53 secondsI don't recommend just creating subagents for the sake of subagents, unless they're very specific ones. I'll show you guys a couple of them in a moment. But for the most part, use them where it makes sense.
3:14:023 hours, 14 minutes, 2 secondsUse them in situations where you want to parallelize the workflow and be a lot faster.
3:14:063 hours, 14 minutes, 6 secondsOkay, and then because we just generated the subagents in a previous instance, we actually have to call the subagents in another Claude Code instance. So I just had to make a new one, basically.
Chapter 30: Scaling Email Classification
3:14:153 hours, 14 minutes, 15 secondsWhat it's gonna do now is spawn a bunch of subagents for me. Okay, so that's what these tasks are.
3:14:183 hours, 14 minutes, 18 secondsSo as you can see, classify email chunk, one, two, three, four, five, six, seven, eight, nine. So all 10 classifiers are now running in parallel. We now have all 10 task outputs here, which is pretty cool.
3:14:283 hours, 14 minutes, 28 secondsIt's now absorbing all these task outputs.
3:14:303 hours, 14 minutes, 30 secondsAnd we're operating at a much higher level of speed than we were before, right?
3:14:353 hours, 14 minutes, 35 secondsSo now it's recording the time, and then it's going to merge and apply.
3:14:383 hours, 14 minutes, 38 secondsClassification took 19 seconds, while clock time for a thousand, sorry, a hundred to complete. And then let's see the total time speed up.
3:14:463 hours, 14 minutes, 46 secondsOkay, so in this instance, because we ran the same number of emails, we did 100 versus 100. We only saved six seconds.
3:14:523 hours, 14 minutes, 52 secondsSo what I wanna show you guys now is I wanna show you guys how to do this at scale.
3:14:573 hours, 14 minutes, 57 secondsSo instead of, you know, a hundred emails, I wanna do a thousand. Excellent work. I'd like you to classify a thousand.
3:15:033 hours, 15 minutes, 3 secondsThe benefits of the speed up are most likely not going to be at the same level of scale, but they will become evident when we go at a much higher level of speed at a scale.
3:15:123 hours, 15 minutes, 12 secondsSo now we're gonna run 1000 of these, aka 1000 emails split into 10 chunks of a hundred that are being classified in parallel with 10 subagents,
3:15:203 hours, 15 minutes, 20 secondsconsidering that every time took, I think, like 19 seconds or something like that per, I think it's gonna be a lot faster. So we'll see.
3:15:273 hours, 15 minutes, 27 secondsOkay, some of these task outputs are now starting to complete. It's been maybe 15, 20 seconds.
3:15:313 hours, 15 minutes, 31 secondsI'm not sure exactly how long, but as they're all coming back, you see these little gray bubbles turn into green bubbles.
3:15:373 hours, 15 minutes, 37 secondsOkay, we ended up having an issue where the prompt was too long, essentially because all of these subagents returned massive strings of text with every single email
3:15:463 hours, 15 minutes, 46 secondsfor whatever reason, when you combine all this into the parent thread, just way too long. And it ran out of context. So what it is, I just copied over everything.
3:15:543 hours, 15 minutes, 54 secondsAnd then I gave it to another instance up here.
3:15:573 hours, 15 minutes, 57 secondsAnd I basically said, hey, this is a little too long right now. I keep running into prompt too long output.
3:16:063 hours, 16 minutes, 6 secondsSo I think we ran out of context. I'd like you to modify this so we don't run out of context.
3:16:093 hours, 16 minutes, 9 secondsIf the subagents don't have to return the actual text to the parent nature, that would be ideal. Then I ran it in parallel for all 10. And now we're just redoing it.
3:16:173 hours, 16 minutes, 17 secondsAt the end of it, it labeled 987 out of 989 emails. I didn't time that end to end.
3:16:233 hours, 16 minutes, 23 secondsIf I had to guess, it'd probably be somewhere around like a minute or so, which means we are now classifying a thousand emails in a minute, whereas 100 was at 36.
3:16:313 hours, 16 minutes, 31 secondsAnd this is really the power of subagents.
3:16:323 hours, 16 minutes, 32 secondsSubagents basically allows us to take some query and then split it up into five, 10, 15, 20, whatever.
3:16:393 hours, 16 minutes, 39 secondsRun them all synchronously at the same time.
3:16:423 hours, 16 minutes, 42 secondsAnd then once they're done, they just take the outputs of each of these threads and then combine them into the main one.
3:16:493 hours, 16 minutes, 49 secondsAnd so, there are a couple of other use cases for subagents, but for the most part, it's going to be something like this.
3:16:533 hours, 16 minutes, 53 secondsLike if you really wanted to use subagents in an economically valuable fashion, this is usually how you would do so as of the time of this recording.
3:16:593 hours, 16 minutes, 59 secondsSubagents are fantastic, but keep in mind, most of the time they're gonna be less intelligent than the parent agent.
3:17:043 hours, 17 minutes, 4 secondsAnd so you wanna reserve the parent agent for taking the outputs of each of these subagents and combining them and doing something with them, not just spawning 500 things in parallel to run for no reason.
3:17:173 hours, 17 minutes, 17 secondsStrategically speaking, some other things about subagents are try and make the task definitions as simple and as straightforward as possible.
3:17:233 hours, 17 minutes, 23 secondsLike I could have given every one of those subagents more context, I could have said, hey, I don't just want you to do the classification, I want you to do everything.
3:17:283 hours, 17 minutes, 28 secondsI want you to do the classifications, the merges, the applying the labels, et cetera.
3:17:313 hours, 17 minutes, 31 secondsBut because the subagents are dumber and because we're spawning a bunch, we're multiplying probabilities here.
3:17:363 hours, 17 minutes, 36 secondsIf there's like even a, I don't know, let's say there's a 95% chance that the subagents gonna work, right? That's a 5% chance that it's not gonna work.
3:17:433 hours, 17 minutes, 43 secondsAnd the way in statistics that you calculate the probability of a bunch of things occurring in sequence is you just multiply them out. So what this is, is this is 0.95 times 0.95 times 0.95.
3:17:533 hours, 17 minutes, 53 secondsBasically what this is equivalent to is 0.95 or raised to the three.
3:17:563 hours, 17 minutes, 56 secondsAnd so if we spawn three subagents, okay, the total probability that all three of them will work the way that we wanted them to, but just go back over here, is 0.95 raised to the three here.
3:18:063 hours, 18 minutes, 6 secondsSo 85, AKA 85.7%.
3:18:103 hours, 18 minutes, 10 secondsYou know, I mean, if I'm running 10, the probability is now down to 59%.
3:18:153 hours, 18 minutes, 15 secondsIf I'm running, I don't know, 50, then the probability is down to 7%.
3:18:183 hours, 18 minutes, 18 secondsObviously I wanna maximize the probability that all of these subagents complete in the time that I've allotted to them and stuff like that. Not only for my own token count issues and my consumption.
3:18:273 hours, 18 minutes, 27 secondsSo you guys see back here, like I'm now at 173 bucks in additional usage on top of my Claude code usage, not just from this course of my idea, but I'm doing a fair amount.
3:18:353 hours, 18 minutes, 35 secondsBut also for like completeness sake, if I malformed the output and then my parent agent can't collect it all right and do something right with it,
3:18:433 hours, 18 minutes, 43 secondswell then what I've done is I've just basically wasted that whole query because subagent prompts are ephemeral. They only exist for like a short period of time. Their context windows are all self-contained.
3:18:513 hours, 18 minutes, 51 secondsDo I really wanna rerun that thing a hundred times, even if it's cheaper? Probably not, right?
Chapter 31: Creating Useful Sub-Agents
3:18:543 hours, 18 minutes, 54 secondsNext up, I wanna show you guys how to create what I'd consider to be the three most useful subagents as of right now.
3:19:003 hours, 19 minutesSo what I'm doing is I'm actually having Claude code create these as we speak.
3:19:033 hours, 19 minutes, 3 secondsOne's called code reviewer, the other is called researcher, and the last one's going to be called QA.
3:19:083 hours, 19 minutes, 8 secondsAnd we're going to insert all three of these agents into this folder here alongside email classifier.
3:19:123 hours, 19 minutes, 12 secondsAnd then I'm gonna update my Claude.md to reference the proposed workflow. Then I'm gonna show you what all that stuff looks like.
3:19:183 hours, 19 minutes, 18 secondsNow, in order to use agents, what we actually have to do is we have to exit a specific instance that we generated the agents in.
3:19:243 hours, 19 minutes, 24 secondsOtherwise we're not gonna see them as available in our task definition. So I'm just going to create a new instance of Claude code. What subagents do we have access to?
3:19:333 hours, 19 minutes, 33 secondsI also refresh this so we could see them all. And as we can see, we have four.
3:19:383 hours, 19 minutes, 38 secondsWe have code reviewer, QA, research, and then email classifier.
3:19:413 hours, 19 minutes, 41 secondsOkay, what is the proposed workflow every time we develop some software?
3:19:503 hours, 19 minutes, 50 secondsWhat I want it to do now is I want it to go through and then tell me, first we write the edit, the code, and the parent agent.
3:19:543 hours, 19 minutes, 54 secondsThen we code review, which spawns a code reviewer subagent and the change falls, fixes any blocking issues.
3:19:593 hours, 19 minutes, 59 secondsThen we do a QA, spawning a QA subagent on the code, generates tests, runs them, reports results, and fixes failures. Then finally we do a ship.
3:20:063 hours, 20 minutes, 6 secondsSo now that we have all that ready, let's actually go and then let's use our new workflow on the flow that we just created before. So I think it was the Gmail-label.
3:20:143 hours, 20 minutes, 14 secondsUse our new workflow on Gmail-label.
3:20:163 hours, 20 minutes, 16 secondsIt's the skill that looks through my inbox and then labels emails.
3:20:213 hours, 20 minutes, 21 secondsSo what I want to do is I want to read through the Gmail-label skill to understand what we're working with. So it's going to read the skill. That's also going to go through all of the scripts.
3:20:293 hours, 20 minutes, 29 secondsBut I basically want us to take these scripts and then apply our little flow.
3:20:333 hours, 20 minutes, 33 secondsSo the first thing it's going to do is run the code review agent on all four scripts.
3:20:363 hours, 20 minutes, 36 secondsAnd as you can see here, we can run these in tandem in parallel.
3:20:393 hours, 20 minutes, 39 secondsSo first we're going to code review, and then we're also going to generate tests and run them for the Gmail-label scripts.
3:20:453 hours, 20 minutes, 45 secondsSo we're going to use both of these, and then we're going to use them to feed back to our parent agent.
3:20:493 hours, 20 minutes, 49 secondsOur parent agent is going to make changes to this code and significantly improve the quality of said code. Now is this like required to do every single time?
3:20:553 hours, 20 minutes, 55 secondsNo, as you guys could see, we're capable of writing some pretty damn good code, but knowing a lick of code, with just like the vanilla Claude Code installation.
3:21:023 hours, 21 minutes, 2 secondsThis sort of stuff becomes more and more valuable when you're working at enterprise and you're creating code that requires the ability to, one, be like really secure and verifiable by both agents and then human beings if they read them.
3:21:133 hours, 21 minutes, 13 secondsAnd then two, to like account for all possible edge cases.
3:21:173 hours, 21 minutes, 17 secondsYou know, in my case, I don't really care too much accounting for all possible edge cases, because most of the software I'm making is for my own internal tooling.
3:21:233 hours, 21 minutes, 23 secondsYou know, it's like a one-off landing page for a client to use, that sort of stuff.
3:21:263 hours, 21 minutes, 26 secondsYou know, if I'm working in a big business, working in a Versel or I'm working in an open AI or I'm working in a, you know, I don't know, Oracle big database or whatnot, this stuff becomes significantly more important.
3:21:343 hours, 21 minutes, 34 secondsAnd that's where these sorts of code design patterns become valuable.
3:21:383 hours, 21 minutes, 38 secondsOkay, so we're still waiting on the output of the other task, but if I scroll down here, you can see there's actually some recommendations already.
3:21:433 hours, 21 minutes, 43 secondsThis is being provided inside of this task output, so it's not written very well or nice.
3:21:483 hours, 21 minutes, 48 secondsSo we're gonna have to squint a bit, but code is correct, readable, it handles errors appropriately.
3:21:533 hours, 21 minutes, 53 secondsBatch fetching uses 100 per batch, but could use the Gmail API max of a thousand requests per batch.
3:21:573 hours, 21 minutes, 57 secondsThat means that we could significantly improve the total efficiency of this flow.
3:22:013 hours, 22 minutes, 1 secondAnd that's one piece of value that the code reviewer has already given for us. Then we have some callback stuff.
3:22:063 hours, 22 minutes, 6 secondsSo basically it's identified an error or an issue, which is quite useful.
3:22:103 hours, 22 minutes, 10 secondsIt's giving us some insights on the readability and, you know, a little commenting that we could be doing to make the code better and so on and so forth. Okay, now the tests are completed.
3:22:193 hours, 22 minutes, 19 secondsSo it looks like we've passed most of the tests. There's only one that had a wrong exception.
3:22:233 hours, 22 minutes, 23 secondsAnd now it's feeding in all of this information to the parent agent. The parent agent is gonna go through and do the fix.
3:22:293 hours, 22 minutes, 29 secondsSo 16 to 18 characters, it's gonna jump through, accepting uppercase and variable length hex IDs.
3:22:353 hours, 22 minutes, 35 secondsNo idea what that means, but of course this agent is now thinking dozens of times faster than I'd be able to. So I'm just gonna trust that it's doing well.
3:22:423 hours, 22 minutes, 42 secondsAnd then front load all of this double checking, triple checking, QA, and so on and so forth to minimize the possibility of longer term errors. So that looks great.
3:22:513 hours, 22 minutes, 51 secondsWe've now run our new flow, which has yielded significantly better benefits. Okay, great.
3:22:563 hours, 22 minutes, 56 secondsNow use the research sub agent to go and find me the best MCP server currently available for PandaDoc.
3:23:063 hours, 23 minutes, 6 secondsSo now I wanna show you guys the value of the research sub agent. This has now spawned one of my research.
3:23:123 hours, 23 minutes, 12 secondsSo it's going through, it's doing tons of research simultaneously, trying a bunch of different search queries and so on and so forth.
3:23:203 hours, 23 minutes, 20 secondsIt's now returned one of the web search results.
3:23:243 hours, 23 minutes, 24 secondsAnd as you can see, it's also doing tons of different like HTTP requests and stuff like that simultaneously.
3:23:293 hours, 23 minutes, 29 secondsNow I should note that like we already technically have a research sub agent built in, but you can modify that research sub agent flow by telling it, hey, you know, I want you to use specific sources.
3:23:373 hours, 23 minutes, 37 secondsI want you to trust these websites.
3:23:393 hours, 23 minutes, 39 secondsI want you to preferentially go directly to the API docs and stuff like that. And so that's what that research sub agent allows us to do.
3:23:453 hours, 23 minutes, 45 secondsIt allows us to research things the way that we typically research things, which is gonna be different from just like doing a general Google request for, I don't know, good APIs for PandaDoc.
3:23:523 hours, 23 minutes, 52 secondsSo again, just to really impress upon you the value of these, really a big chunk of value is it's cheaper to use Sonnet as of right now versus Opus.
3:24:063 hours, 24 minutes, 6 secondsAnd so rather than do your research or do your low, you know, leverage or low ROI stuff, like reading through a large amount of data
3:24:143 hours, 24 minutes, 14 secondsto extract something, it's better to use the cheaper models.
3:24:173 hours, 24 minutes, 17 secondsThe next is that it's parallelizable, which just means that you can spin up multiple simultaneously and then wait for all their inputs as opposed to going one at a time.
3:24:263 hours, 24 minutes, 26 secondsLike for instance, if this is us, and this is sort of our task flow, let's say, you know, this is sort of the serial method, which is what we were doing before. Let's say every search takes one minute.
3:24:353 hours, 24 minutes, 35 secondsSo, you know, this is task one, takes one minute. This is task two, which takes one minute. And then this is task three, which takes one minute.
3:24:453 hours, 24 minutes, 45 secondsI guess this is two and then this is three.
3:24:473 hours, 24 minutes, 47 secondsThat means in order to get to, you know, the start of our query to the end of our query, Claude Code takes three minutes in total, right?
3:24:573 hours, 24 minutes, 57 secondsWell, obviously the parallel approach here is a lot better.
3:25:013 hours, 25 minutes, 1 secondWe start, and then what we do is we just spin up three different boxes here simultaneously.
3:25:093 hours, 25 minutes, 9 secondsAnd now these all take one minute.
3:25:123 hours, 25 minutes, 12 secondsAnd, you know, by the time that we end, what we've done is we've basically taken one minute because each of these are executing side long sort of with each other.
3:25:203 hours, 25 minutes, 20 secondsThe last major benefit is the way that the context works.
3:25:233 hours, 25 minutes, 23 secondsAnd so there's some situations like a, you know, reviewer sub agent where it's actually beneficial not to have any of the context of the code.
3:25:303 hours, 25 minutes, 30 secondsIt's not to have any of the biases of the decision-making of the previous parent agent.
3:25:343 hours, 25 minutes, 34 secondsAnd sometimes, you know, choosing a different model to do some of the reasoning can, you know, reveal things that maybe the parent agent didn't necessarily think of.
3:25:433 hours, 25 minutes, 43 secondsSometimes it makes more sense to look at the ground at your feet, for instance, the dumbness, rather than look up in the sky at like all the complex advanced stuff.
3:25:503 hours, 25 minutes, 50 secondsSame thing with sort of like a QA agent.
3:25:523 hours, 25 minutes, 52 secondsThe value of both of these is they don't necessarily know what's going on in terms of the broader world.
3:25:573 hours, 25 minutes, 57 secondsAll they're really focused on is the code itself, the way that it was written.
3:26:013 hours, 26 minutes, 1 secondAnd so they get to optimize objectively at like the way to make that thing as efficient as possible. And that's sub agents in a nutshell. It doesn't have to be any more complicated than that.
3:26:083 hours, 26 minutes, 8 secondsIt's basically just a folder structure and it's very similar to skills.
3:26:113 hours, 26 minutes, 11 secondsMy recommendation is use this in conjunction with things like skills to accomplish preexisting workflows many times faster because of parallelization.
3:26:193 hours, 26 minutes, 19 secondsBut don't rely on sub agents because a lot of the time, the time it takes to spin up a sub agent for a simple query can be just as long as it would take to use just a parent agent to do the thing instead.
Chapter 32: Understanding Agent Teams
3:26:283 hours, 26 minutes, 28 secondsWhile sub agents sound really sexy, and obviously everybody wants to have giant fleets and swarms of them working for you on your behalf, be pragmatic and be efficient here.
3:26:363 hours, 26 minutes, 36 secondsAnd now it's time to discuss one of Claude Code's most commonly hyped and misunderstood, but also pretty powerful features, agent teams.
3:26:443 hours, 26 minutes, 44 secondsIf you're unaware, Claude Code recently unveiled new functionality where you can orchestrate a team of agents and you actually don't do the orchestration yourself.
3:26:523 hours, 26 minutes, 52 secondsYou can actually spin up a team of agents that are managed by another agent for you.
3:26:573 hours, 26 minutes, 57 secondsAnd then all you really need to do is just report back to that manager agent, let them know what you want to do and so on and so forth.
3:27:023 hours, 27 minutes, 2 secondsObviously, given the fact that this is pretty interesting at first glance, a lot of people are pretty stoked about it and they've made tons of videos talking all about how agent teams run their whole life and have revolutionized programming and so on and so forth.
3:27:133 hours, 27 minutes, 13 secondsHopefully in this module, I'm going to show you that this is more of the same and agent teams are just another way that you can parallelize your workflow.
3:27:193 hours, 27 minutes, 19 secondsSo the way I want you to think about agent teams are basically just a more advanced version of sub-agents.
3:27:253 hours, 27 minutes, 25 secondsBasically, both agent teams and sub-agents are a mechanism of parallelization.
3:27:303 hours, 27 minutes, 30 secondsLike we had earlier when I showed you that example of doing a bunch of classification, we have a task and we could do the task one by one.
3:27:403 hours, 27 minutes, 40 secondsAnd if we do the task one by one, what we're doing is we're incurring a fair amount of fixed time cost.
3:27:453 hours, 27 minutes, 45 secondsNot to mention there are some instances where task steps aren't even necessary.
3:27:493 hours, 27 minutes, 49 secondsAnd so if each of these are one minute, obviously that's one minute plus one minute plus one minute equals a total time of three minutes to complete the task.
3:27:573 hours, 27 minutes, 57 secondsMultiply this by 60, you get an hour, an hour, an hour, three hours. I'm sure you can start understanding why we parallelize work.
3:28:053 hours, 28 minutes, 5 secondsMuch better to be able to spin up three separate solutions, have those operate simultaneously and then merely integrate their results into one thread.
3:28:153 hours, 28 minutes, 15 secondsOkay, in a situation like this, assuming one, two, three, take one minute, obviously the total time spent is about one minute.
3:28:213 hours, 28 minutes, 21 secondsSo just like subagents allows one agent to spin up a bunch of these different tasks and then parallelize them, so too do agent teams.
3:28:283 hours, 28 minutes, 28 secondsIt's just they operate one level even higher.
3:28:313 hours, 28 minutes, 31 secondsInstead of splitting one thread into three, what you end up doing is you basically end up splitting as many threads as you want into as many sub threads as you want as well.
3:28:423 hours, 28 minutes, 42 secondsAnd so in this specific case, basically, I have one what's called team lead agent.
3:28:493 hours, 28 minutes, 49 secondsAnd this team lead agent, as opposed to doing one, two, three, four, five, and six himself, what he's doing is he's splitting things up
3:28:583 hours, 28 minutes, 58 secondsinto two separate agents here, having them both run three subagents on their own and then combine that into one call.
3:29:073 hours, 29 minutes, 7 secondsAt the end of it, this agent then combines them back into the main thread and then can reason about things and so on and so forth.
3:29:133 hours, 29 minutes, 13 secondsMuch in the same way that if you think about it, organizational hierarchies work.
3:29:193 hours, 29 minutes, 19 secondsYou'll have like a manager up here in a business, and then you'll have, for lack of better words, like grunts down at the bottom.
3:29:273 hours, 29 minutes, 27 secondsThe manager tells the grunt what to do. The grunt goes, does what they want, and then reports back.
3:29:333 hours, 29 minutes, 33 secondsSo too, do we have sort of the same system with subagents and now manager agents as well.
3:29:393 hours, 29 minutes, 39 secondsAnd then you basically sit outside of this whole thing, just watching it all occur and then nudging different people within the organization or letting the team lead know you wanna change something where necessary.
3:29:493 hours, 29 minutes, 49 secondsSo if you break things down, subagents own all of the context window and the results return to the agent that called them.
3:29:573 hours, 29 minutes, 57 secondsSo in our case, we have a parent agent, we have a child agent.
3:29:593 hours, 29 minutes, 59 secondsOur child agent owns its own context window and the results, every time, always go directly to the person or the agent, I should say.
3:30:073 hours, 30 minutes, 7 secondsLook at me, anthropomorphize and these damn things that called it. On agent teams, they own their own context window completely. They're also fully independent.
3:30:153 hours, 30 minutes, 15 secondsAnd so they don't necessarily have to return their results back to the caller. In fact, agent teams can communicate between them.
3:30:223 hours, 30 minutes, 22 secondsSo earlier, where we saw the grunts communicating with the manager, grunts also have the ability to basically communicate between each other.
3:30:293 hours, 30 minutes, 29 secondsAnd while I think that this is ultimately less powerful or less effective than communicating with the manager because the manager is responsible for synthesis, there are some instances where, you know,
3:30:383 hours, 30 minutes, 38 secondsGrunt 1 does have a interesting revelation or time saver for Grunt 2 that could save them a fair amount of time.
3:30:443 hours, 30 minutes, 44 secondsAnd in that way, this sort of cross contamination and cross pollination of ideas, while consuming significantly more tokens can lead to a better quality final product.
3:30:523 hours, 30 minutes, 52 secondsAnd that takes us to communication, right?
3:30:543 hours, 30 minutes, 54 secondsWith sub-agents, you always report back to the main agent only, but here teammates can message each other directly.
3:30:593 hours, 30 minutes, 59 secondsBasically, what occurs in an agent team is they build this mutual scratch pad, which is almost like a message board or a BBS board, if you guys remember from way back in the day.
3:31:073 hours, 31 minutes, 7 secondsIt's like a forum, it's like they're on mini Reddit and they'll post tasks that they're currently working on.
3:31:123 hours, 31 minutes, 12 secondsIf they have any questions, they'll ask specific people that are responsible for those things. And they'll always just have that stored in their context.
3:31:183 hours, 31 minutes, 18 secondsSo if they, you know, have a question from one person, they can prioritize that question and then go and find it in another context and immediately reply.
3:31:263 hours, 31 minutes, 26 secondsIn that way, you could save individual agents significant amounts of time.
3:31:303 hours, 31 minutes, 30 secondsIn terms of coordination here, the main agent manages all the work, but with agent teams, it's a shared task list with self-coordination.
3:31:363 hours, 31 minutes, 36 secondsSo just like we had a little Trello board or maybe, you know, a click up task list or something like that. So too do these agents work off the JSON equivalence.
3:31:453 hours, 31 minutes, 45 secondsThey say that sub-agents are best for focused tasks where only one result matters, whereas agent teams are best for complex work requiring discussion and collaboration.
3:31:533 hours, 31 minutes, 53 secondsYou know, this is just one of those like little marketing isms.
3:31:553 hours, 31 minutes, 55 secondsThe definition between focused task and complex task is very, very fuzzy, and there is no real delineation between them.
3:32:023 hours, 32 minutes, 2 secondsIt's sort of like how there's a certain point at which a hill becomes a mountain, but nobody could tell you exactly how many feet high the hill needs to be or whatever, right?
3:32:103 hours, 32 minutes, 10 secondsIt's just one of those things where when you know, you know.
3:32:123 hours, 32 minutes, 12 secondsAnd finally, the token costs of sub-agents are quite low, relatively speaking, whereas agent teams are very, very high because every teammate is actually a whole separate clot instance.
3:32:213 hours, 32 minutes, 21 secondsSo when you scale up and spin up a bunch of these, you can use a fair amount of tokens quite quickly.
3:32:253 hours, 32 minutes, 25 secondsNow I should note that agent teams are not enabled by default because they're what we call an experimental feature.
3:32:313 hours, 32 minutes, 31 secondsThis may not necessarily be true by the time you're watching, by the way, but for now they are. They have set them to off essentially by default.
3:32:393 hours, 32 minutes, 39 secondsAnd so only advanced users really get to peer behind the curtain and use them.
3:32:433 hours, 32 minutes, 43 secondsThe way that you enable them is you edit your settings.json in your current workspace, and you just create this sort of little string.
3:32:503 hours, 32 minutes, 50 secondsYou have this curly brace, you have env, env, you have clod code, experimental agent teams, you set that to one, and then you have some closing curly brackets. You don't need to worry too much about that.
3:32:583 hours, 32 minutes, 58 secondsWe'll do that in like five seconds.
Chapter 33: Enabling and Using Agent Teams
3:32:593 hours, 32 minutes, 59 secondsFinally, the cool thing about agent teams as mentioned is you can't just, it's not only that you can communicate with the parent agent, you can communicate with all of the agents.
3:33:073 hours, 33 minutes, 7 secondsSo if, I don't know, you want to context switch and tell agent three in the sequence to do something different than it was currently doing, you can absolutely do that really easily.
3:33:163 hours, 33 minutes, 16 secondsThere are two different ways to do so.
3:33:173 hours, 33 minutes, 17 secondsThere's what's called in process mode and then split pane mode, at least this is the time of this recording.
3:33:213 hours, 33 minutes, 21 secondsOne is where you basically just like alt tab through all of them. The other is where there's just multiple panes.
3:33:263 hours, 33 minutes, 26 secondsAnd so you'll see agent one over here, you'll see agent two over here, agent three over here, and then you'll kind of get their feed.
3:33:323 hours, 33 minutes, 32 secondsI will note, I've done this before, unfortunately, because Claude Code renders your, it's not just like a simple text terminal.
3:33:383 hours, 33 minutes, 38 secondsBasically they're like rendering this 2D image constantly on your screen. It can consume a fair amount of compute. So I don't actually like using it that way anymore.
3:33:463 hours, 33 minutes, 46 secondsI basically always use it in process, but I'll run you through what that looks like if you did want to use split pane mode.
3:33:513 hours, 33 minutes, 51 secondsAnd then obligatory agent team tokens cost way more because you're spawning tons of different cloud instances.
3:33:573 hours, 33 minutes, 57 secondsAnd every single one has its own context window and can do its own stuff.
3:34:003 hours, 34 minutesSo if you have like 10 active agents running, you're gonna consume about 10 times the context, if not more, because there's also gonna be some coordination and communication lag and overhead.
3:34:093 hours, 34 minutes, 9 secondsSo they have some recommendations here.
3:34:103 hours, 34 minutes, 10 secondsThey say use Sonnet for teammates, keep teams really small because every teammate runs its own context window. So token usage is roughly proportional to team size. Keep the spawn prompts focused.
3:34:203 hours, 34 minutes, 20 secondsWe don't know what those are. So I'll tell you that in a second.
3:34:223 hours, 34 minutes, 22 secondsTeammates will load their own cloud MD, MCP servers and skills automatically, but everything in the spawn prompt will also add to their context from the start. Clean up teams when the work is done.
3:34:313 hours, 34 minutes, 31 secondsSo you can actually roll them down or shut them down.
3:34:333 hours, 34 minutes, 33 secondsAnd then, yeah, agent teams are disabled by default because they don't want anybody blowing $10,000 on agent teams in the day, which you can absolutely do if you're not careful.
3:34:413 hours, 34 minutes, 41 secondsThat's why limits are so important.
3:34:433 hours, 34 minutes, 43 secondsOkay, so first things first, we have to actually enable agent teams. So I'm just gonna jump over here to this URL. And then I'm just gonna copy all of the text on this page.
3:34:513 hours, 34 minutes, 51 secondsAnd then I'm gonna go over to anti-gravity, open that puppy up.
3:34:543 hours, 34 minutes, 54 secondsAnd just for the purposes of this example, I'm actually just gonna open a new folder. So go to my Mac and then I'll say agent teams example.
3:35:023 hours, 35 minutes, 2 secondsOkay, and I open that.
3:35:043 hours, 35 minutes, 4 secondsAnd then what I'm gonna do is go over to cloud, paste this in, and I'm gonna say enable agent teams.
3:35:123 hours, 35 minutes, 12 secondsI'm gonna go bypass permissions, close this puppy out so we can all see it.
3:35:153 hours, 35 minutes, 15 secondsMaybe bump this out a bit so you guys can always see the text.
3:35:193 hours, 35 minutes, 19 secondsSo it's now added my settings.json here and it's kind of fixed it. Okay, so this is now good to go. And it's enabled this across my global workspace.
3:35:283 hours, 35 minutes, 28 secondsSo it's not actually even in my file. Let's start with a really simple example of agent teams. So I could show you the parallelization aspect.
3:35:333 hours, 35 minutes, 33 secondsAnd then what we'll do is we'll actually go into an open source code base.
3:35:373 hours, 35 minutes, 37 secondsAnd I'll use agent teams to act as both code reviewers and then also debaters to debate between each other until they determine that consensus on how to make the code even better.
3:35:463 hours, 35 minutes, 46 secondsSo our first super simple example is gonna be, I'm designing a simple personal website for Nix or Iave.
3:35:513 hours, 35 minutes, 51 secondsGenerate three agents using a team and create three fundamentally different designs.
3:35:543 hours, 35 minutes, 54 secondsOpen all three once done and I'll compare, contrast and give feedback.
3:36:013 hours, 36 minutes, 1 secondAlso make sure they know everything there is to know about me so nobody is waiting on anything.
3:36:113 hours, 36 minutes, 11 secondsSo I'm using a terminal for this just because the terminal UX is much nicer for agent teams than the GUI UX.
3:36:163 hours, 36 minutes, 16 secondsI'm sure that'll change at some point, but yeah, I also have fast mode on up here, which is just allowing me to do this a little bit faster.
3:36:223 hours, 36 minutes, 22 secondsAnd so as you see, what's occurred is the agent, this parent agent here, Opus 4.6, sort of made the executive decision for its very first task, basically.
3:36:323 hours, 36 minutes, 32 secondsOh, that's so cool, I didn't even know I could do this, to do research on Nix.
3:36:363 hours, 36 minutes, 36 secondsAnd so after it's done the research, basically, it's now gonna spin up three agents, one for site one, another for site two and another for site three.
3:36:453 hours, 36 minutes, 45 secondsI really gotta figure out how to do this with hotkeys, it's super annoying.
3:36:493 hours, 36 minutes, 49 secondsAnd then these three agents are gonna go working on this thing simultaneously and independently and then they're going to combine those three websites back into just like we did with subagents, sort of that main thread.
3:36:593 hours, 36 minutes, 59 secondsBut what's cool is these three different agent teams, since they're all individual Claude Code instances, they get to do a variety of different things.
3:37:063 hours, 37 minutes, 6 secondsThey also get to access their own agents, use their own code base and stuff like that.
3:37:103 hours, 37 minutes, 10 secondsSo what's really cool is we have three agents now working in parallel.
3:37:143 hours, 37 minutes, 14 secondsThe first is called Design Minimalist, the second is called Design Dark, and the third is called Design Warm.
3:37:193 hours, 37 minutes, 19 secondsI asked for fundamentally different types of designs, which is why they're doing this.
3:37:223 hours, 37 minutes, 22 secondsNow, if you wanted to see all these agents run simultaneously, all you'd have to do is just go shift up or down.
3:37:273 hours, 37 minutes, 27 secondsAnd so right now I'm in the Team Lead context, but I could actually go down here and then press enter. And now I'm in the Design Dark.
3:37:343 hours, 37 minutes, 34 secondsAs you see here, we got a ton of information here with some context about who NixRyf is, and then it says you're designing a personal website, create a single self-contained file.
3:37:413 hours, 37 minutes, 41 secondsIt's now creating a bold dark tech website.
3:37:443 hours, 37 minutes, 44 secondsWe could also go up to the main Team Lead and then you could see that it's let me know that the Design Minimalist is done and it's still waiting on Design Dark and Design Warm to finish their builds.
3:37:533 hours, 37 minutes, 53 secondsSo I mean, like how exactly is this different from, I don't know, like sub-agents right now?
3:37:583 hours, 37 minutes, 58 secondsWell, it's different from sub-agents in that you can treat every one of these as basically having its own whole Clawed code instance available to it, okay?
3:38:063 hours, 38 minutes, 6 secondsWhereas before every individual sub-agent only had access to the contacts that the parent agent gave, realistically, what I could do is I could add a Clawed.md,
3:38:143 hours, 38 minutes, 14 secondsand all three of these would have access to Clawed.md, you know, style guides and stuff like that. So I'm gonna take a look at this, okay?
3:38:213 hours, 38 minutes, 21 secondsSaying that it's all done now, and it actually shut down all three of those agents as well, which is really, really important.
3:38:263 hours, 38 minutes, 26 secondsIf they're constantly running in the background, you're also gonna be consuming compute resources just as well as UR tokens. Now I'm gonna compare which ones I like more.
3:38:353 hours, 38 minutes, 35 secondsThis one up here is building at the intersection of AI and human ambition. Wow, look at that, that's nice. Geez, insane. It's got a couple things wrong here.
3:38:433 hours, 38 minutes, 43 secondsDefinitely have more than 150K YouTube sales, but what are you gonna do? Looks like it does have my links, which is kinda cool. This is like, you know, dark coding one.
3:38:513 hours, 38 minutes, 51 secondsLook at that, isn't that neat? And this one over here is very interesting.
3:38:563 hours, 38 minutes, 56 secondsThere's no picture of me on it, but hey, what are you gonna do? That was my little nightclub promotions party. Business, that's a hell of a throwback.
3:39:043 hours, 39 minutes, 4 secondsAnd yeah, what happens if I click this? Okay, cool, we go back to our YouTube. That's really exciting. So I mean, like, I don't know, maybe, hmm.
3:39:123 hours, 39 minutes, 12 secondsMaybe I really like the first one.
3:39:163 hours, 39 minutes, 16 secondsSo now what I'm gonna do is I'm gonna go back and I'm gonna say, hey, I really liked the warm narrative option, looks great. I'd like you now to spin up three more agents.
3:39:253 hours, 39 minutes, 25 secondsI then want you guys to do research on effective design principles and copywriting principles that convert.
3:39:363 hours, 39 minutes, 36 secondsOnce done, I want you to spin up a bunch of agents to iterate on this design and come up with their own flavors or versions, then report back to me.
3:39:443 hours, 39 minutes, 44 secondsSo now what I'm doing is I'm taking, you know, this winning design here, the warm one, it's gonna take this warm, beautiful thing, and then I basically want it to like iterate on it even more.
3:39:533 hours, 39 minutes, 53 secondsAnd as you saw, this occurred pretty quickly, right? I mean, this took me maybe like two minutes or so. Is it perfect?
3:39:583 hours, 39 minutes, 58 secondsNo, but because it's not perfect, I basically just wanna have Claude do a bunch of iterations on it and then give me what I consider to be an even better version, which I think it can do pretty quick.
3:40:063 hours, 40 minutes, 6 secondsSo it's gonna spin up a bunch more.
3:40:083 hours, 40 minutes, 8 secondsWe have research, copy, research, design, and research examples. This is a good, you know, actual use case here. It's doing three research agents in parallel.
3:40:173 hours, 40 minutes, 17 secondsWe have one that's figuring out like strong design principles based off of, you know, winning combinations. There's another that's doing some copywriting fundamentals.
3:40:253 hours, 40 minutes, 25 secondsAnd then the third that is looking for high quality creator sites. So Ali Abdel, guy that I like, Hormozi, obviously. Dan Coe, these guys are perfect.
3:40:333 hours, 40 minutes, 33 secondsIt's more or less exactly what I'm looking for. So it's gonna go do a bunch of research on them.
3:40:363 hours, 40 minutes, 36 secondsAnd then it's going to incorporate that in presumably some other type of designer. I could see the status by going shift up and down.
3:40:423 hours, 40 minutes, 42 secondsSo this person here, research copy, it's looking up, I don't know, best hero copy formulas, personal brand,
3:40:503 hours, 40 minutes, 50 secondsscannable web copy best practices, David Ogilvie copywriting principles, headlines that work, right?
3:40:563 hours, 40 minutes, 56 secondsIf I go down here to research examples, this agent is now writing up a bunch of high quality website styles.
3:41:033 hours, 41 minutes, 3 secondsIt's then analyzing the websites and, you know, giving me all of the copy and stuff like that.
3:41:073 hours, 41 minutes, 7 secondsPresumably it's going to integrate this into something nice.
3:41:103 hours, 41 minutes, 10 secondsBut if I go back up to the team lead, then I can see that it's, you know, basically just waiting on all three of these to finish.
3:41:153 hours, 41 minutes, 15 secondsWhat's cool is these three all have their own token usages, as you see here.
3:41:193 hours, 41 minutes, 19 secondsSo 53,000, this one here is 56,000, this one here is 50,000.
3:41:223 hours, 41 minutes, 22 secondsWhen they finish, it then says idle and it tells you how many seconds that the agent is idle.
3:41:273 hours, 41 minutes, 27 secondsThis is, you know, mildly useful, obviously not utilizing your cloud agents is one of like the biggest issues with them.
3:41:333 hours, 41 minutes, 33 secondsSo what this thing is going to do is basically wait for these other two to finish.
3:41:363 hours, 41 minutes, 36 secondsAnd then if these other two don't finish after a certain amount of time, they'll actually just wind down the research design agent to stop consuming my compute and stuff.
3:41:433 hours, 41 minutes, 43 secondsProbably the research examples is going to take the longest time, just because I think that that's like less of a clear cut definition of done, but we'll see.
3:41:503 hours, 41 minutes, 50 secondsAnd then what's really nice is these are cheaper models, right?
3:41:533 hours, 41 minutes, 53 seconds58,000 tokens on the cheaper model, 56,000 tokens on the cheaper model, then only 2000 tokens on the most expensive model.
3:42:003 hours, 42 minutesAnd so we haven't actually integrated all of that stuff back into the main yet, but as these finish, they will eventually, you know, take all of their tokens and then bring them back in.
3:42:083 hours, 42 minutes, 8 secondsAnd so this token count will increase significantly.
3:42:113 hours, 42 minutes, 11 secondsOkay, now that all three of these are done, we've collapsed these three agents into the team lead.
3:42:173 hours, 42 minutes, 17 secondsNow we have these big design principles doc, research copywriting doc, you know, research site example doc.
3:42:233 hours, 42 minutes, 23 secondsAnd because I've empowered the team lead to be able to spin up new agents based off of, you know, various things like the conversion rate,
3:42:313 hours, 42 minutes, 31 secondsthe copy, the creative and the style, it's now generating new ones. So there's an iterate dark, iterate conversion.
3:42:393 hours, 42 minutes, 39 secondsI don't know how many of these it's going to spin up, but it's definitely gonna spin up some.
3:42:433 hours, 42 minutes, 43 secondsIn the meantime, we also have these really dense research summaries. So I can actually open this research design principles doc.
3:42:493 hours, 42 minutes, 49 secondsIf I just scroll down a bit so you could see, we now have things like there's a Z pattern layout.
3:42:553 hours, 42 minutes, 55 secondsSince the eye starts in the top left and moves to the top right, your nav and CTA should be at particular places.
3:43:003 hours, 43 minutesThere's also an F pattern layout and different actionable recommendations on color psychology and so on and so forth. I mean, this is a tremendous amount of text. Is this the most efficient way to like get all this across?
3:43:093 hours, 43 minutes, 9 secondsProbably not.
3:43:103 hours, 43 minutes, 10 secondsBut because these models just think so much quicker than we do at this point, we don't really need it to be as efficient.
3:43:143 hours, 43 minutes, 14 secondsWe can actually just throw a tremendous amount of text out of prompt and it can actually do a pretty good job.
3:43:183 hours, 43 minutes, 18 secondsWhat I really like about this is it took key inspirations from different people. So in this case, Justin Welsh's upside down homepage. I really liked Justin Welsh, so that's great.
3:43:263 hours, 43 minutes, 26 secondsDan Coe and Seahill Bloom's dark premium, bold typography, origin story is some sort of cinematic centerpiece. Like it's taken inspiration from all these different people.
3:43:343 hours, 43 minutes, 34 secondsThen it's combining them with slightly different copywriting directions to create things that are ultimately new and presumably going to be quite different.
3:43:423 hours, 43 minutes, 42 secondsAnd you know, I think a lot of people rag on agents and AI is not really being creative.
3:43:463 hours, 43 minutes, 46 secondsBut like what is creativity if not just like combining things over and over and over again in like a million different combinations.
3:43:523 hours, 43 minutes, 52 secondsI'd wager most things that you probably consider to be creative are things that like whose preexisting pieces and principles existed before
3:43:593 hours, 43 minutes, 59 secondsand AI just combined them into something that maybe hadn't really been put together in that way.
3:44:033 hours, 44 minutes, 3 secondsThere are certain sentences that have never been said before or written before. You could be the first to write one. Some of them are quite creative. Okay, and we are done.
3:44:103 hours, 44 minutes, 10 secondsSo the first site here is I hope aspiring entrepreneurs build their first 25k month automation agency. I like this, this is really clean. That's actually quite the value prop. We do have an issue with the button, obviously.
3:44:203 hours, 44 minutes, 20 secondsSo the thing is this model was not given the ability to screenshot.
3:44:243 hours, 44 minutes, 24 secondsI bet you if I did, that would have been pretty straightforward.
3:44:273 hours, 44 minutes, 27 secondsSo this is a good opportunity for me to update the cloud.md and say, hey, you know, you can do some screenshots. Still, this looks really great. Step one, watch the free training.
3:44:343 hours, 44 minutes, 34 secondsStep two, join maker school. Step three, build your agency.
3:44:363 hours, 44 minutes, 36 secondsI mean, honestly, the fact that this is just a couple of minutes, this is so much better than what I could have done in an equivalent amount of time. It's not even funny.
3:44:443 hours, 44 minutes, 44 secondsAnd not only did I generate one, I generated three. It's not the dark one, right? That's pretty clean. I like that. This must be the Danco one.
3:44:533 hours, 44 minutes, 53 secondsThere has to be a faster way to matter. Ooh, that's clean, right?
3:44:583 hours, 44 minutes, 58 secondsAgain, it's taken that main website and then it's iterated on it based off of different styles and different approaches, which is more or less exactly what you do in any sort of copywriting and so on and so forth.
3:45:083 hours, 45 minutes, 8 secondsSo I really like this one. I mean, this one to me is probably my favorite.
3:45:113 hours, 45 minutes, 11 secondsYou wanna build an automation business, but you don't know where to start. I really, really like that. So I think I'm actually gonna take that. Great work. Build in screenshot functionality.
3:45:203 hours, 45 minutes, 20 secondsBecause you don't have the ability to screenshot, a couple of things stand out. Also get pictures of me to put on the site.
3:45:293 hours, 45 minutes, 29 secondsLet's choose the first one, which is the conversion machine. I think it's the conversion machine, right? Yep.
3:45:373 hours, 45 minutes, 37 secondsAlso, we need to update some stats. We have 2,100 or 2,200 people in Maker School right now. You can check that out just by Googling Maker School.
3:45:463 hours, 45 minutes, 46 secondsAnd then, let me see, what else do we have?
3:45:483 hours, 45 minutes, 48 secondsGrab the image of me, Alex Hormozi, and Sam Ubbins and put that somewhere on the site and add more to the site.
3:45:543 hours, 45 minutes, 54 secondsRight now it's good, but I want it longer and with pictures of me.
3:46:013 hours, 46 minutes, 1 secondAlso see if you could build some sort of animation on the main homepage.
3:46:053 hours, 46 minutes, 5 secondsI think that would add a significant amount of visual appeal. Right now it's pretty vague. I do quite like the gradient though.
3:46:133 hours, 46 minutes, 13 secondsWhile you're at it, spin up another three and continue doing more iterations, more sections, etc.
3:46:193 hours, 46 minutes, 19 secondsOkay, as you can see, we're now searching significantly more of the total space of possible websites here.
3:46:253 hours, 46 minutes, 25 secondsBecause not only did I spin up three initially to build me these and then three iterations on the one that I liked, I'm now spinning up another four
3:46:333 hours, 46 minutes, 33 secondsbased off of the one that I really liked from that previous iteration.
3:46:383 hours, 46 minutes, 38 secondsAnd so in that way, if you think about it, what we're doing is we're taking this idea of what I want. We're testing a few variants.
3:46:463 hours, 46 minutes, 46 secondsWe're seeing which ones actually look good. These are two no's. We're spinning up even more. We're seeing which ones of these I really like.
3:46:553 hours, 46 minutes, 55 secondsOkay, these ones are all now. And now we're spinning up another four.
3:46:593 hours, 46 minutes, 59 secondsAnd eventually if you continuously do this process, you'll get to a website or a web app or a property that's like five times better.
3:47:123 hours, 47 minutes, 12 secondsBecause we have essentially, instead of just picking one option and stopping there, we've really thoroughly explored the space of all possible opportunities and options.
3:47:213 hours, 47 minutes, 21 secondsAnd so that's something that agent teams really help with. I know my tech's kind of slanting up, but bear with me.
3:47:273 hours, 47 minutes, 27 secondsAnd while I'm doing that, we see that some of these are already starting to finish. So iterate scroll, just finished. That was pretty fast.
3:47:323 hours, 47 minutes, 32 secondsLooks like the editorial magazine style site completed with all 13 sections as well.
3:47:363 hours, 47 minutes, 36 secondsWe got the conversion site fully rebuilt with all the changes now. And now we're just waiting on the split. There it is. It's now gonna open all four of these.
3:47:443 hours, 47 minutes, 44 secondsAnd so for 150,000 tokens or whatever the hell I just spent essentially, I've now been able to draft up what I consider to be a pretty clean and sexy website.
3:47:543 hours, 47 minutes, 54 secondsI have that picture that I was looking for. I have this. I mean, this is great, right?
3:47:583 hours, 47 minutes, 58 secondsOne thing I'm missing is that little video page, but hopefully it's clear.
3:48:023 hours, 48 minutes, 2 secondsI mean, I could build, God, websites are just the tip of the iceberg in terms of what you could build. Have my picture here. I got my whole business partner, Shoma.
3:48:103 hours, 48 minutes, 10 secondsThat's me during COVID doing a little videography shoot. That's when we played bowling down in the Philippines. I mean, like the stuff's super straightforward.
3:48:173 hours, 48 minutes, 17 secondsAlso, I really like this other editorial site. I might end up just choosing that. That's super clean, right? Those harsh corners and the photos and stuff, very nice.
3:48:253 hours, 48 minutes, 25 secondsOkay, but if you just use Agent Teams to design a bunch of websites, you're honestly leaving tons of potential on the table. Most people are kind of uncreative.
3:48:333 hours, 48 minutes, 33 secondsAnd so obviously most of the demos you're gonna see on the internet are gonna be like, watch me rebuild this website 400 ways like I just showed you. You can go a lot deeper than that.
3:48:403 hours, 48 minutes, 40 secondsAnd actually the number one recommended use for Agent Teams right now, at least my recommended use is using it on pre-existing repos to do a tremendous amount of research in a short period of time.
3:48:493 hours, 48 minutes, 49 secondsSo what I'm gonna do next is I'm actually gonna go over here.
3:48:503 hours, 48 minutes, 50 secondsI'm gonna delete all of these websites that I built because the websites are basically only worth the tokens that they're printed on, okay?
3:48:573 hours, 48 minutes, 57 secondsI'm gonna full screen this and then I'm gonna say, clone this and then open an anti-gravity instance inside of it.
3:49:043 hours, 49 minutes, 4 secondsThen I'm gonna paste in one of the repos for OpenClaw, which I've made some rather scathing videos of.
3:49:113 hours, 49 minutes, 11 secondsOpenClaw is totally open source, which means you can muck around the code, take a peek at the way that things were written, make improvements if you really wanted to and so on and so forth.
3:49:203 hours, 49 minutes, 20 secondsLooks like it doesn't know what anti-gravity is. Anti-gravity, the app, it's like VS code.
3:49:273 hours, 49 minutes, 27 secondsOkay, cool and it ended up opening up OpenClawed inside of this. As you see, we are now like in the folder of OpenClawed.
3:49:353 hours, 49 minutes, 35 secondsIt's just instead of it being on GitHub, it's now on our computer. It's just a quick and easy hack.
3:49:403 hours, 49 minutes, 40 secondsYou can basically do whatever the heck you want with these repos.
3:49:423 hours, 49 minutes, 42 secondsOnce you're inside, I'm just gonna open in the terminal because I think I can probably go significantly faster in the terminal. Let me just open this up. Okay, I have it right down over here.
3:49:513 hours, 49 minutes, 51 secondsThen I'm gonna pop this puppy open, make it go full screen by clicking that button on the top right.
3:49:573 hours, 49 minutes, 57 secondsAnd now I'm going to use agent teams to go through this massive, massive file and then make improvements.
3:50:033 hours, 50 minutes, 3 secondsTaking a look at this prompt, I've wrote OpenClaw is great, but there are a bunch of security issues.
3:50:073 hours, 50 minutes, 7 secondsI don't know exactly where they all are yet, but I want you to find them.
3:50:093 hours, 50 minutes, 9 secondsFirst, create a team with 10 teammates to look through the code base very quickly. Split things up logically based on file size, et cetera.
3:50:163 hours, 50 minutes, 16 secondsThen spin up four agents to document all of the security issues and a fifth and sixth debate agent that plays devil's advocate back and forth.
3:50:243 hours, 50 minutes, 24 secondsUse Sonnet for each teammate so as to make use of the longer context window.
3:50:273 hours, 50 minutes, 27 secondsWhen you've identified all the security issues and then spin up one agent per security issue and make changes.
3:50:323 hours, 50 minutes, 32 secondsEnsure each agent works only on the security issue it is given to minimize overlap.
3:50:353 hours, 50 minutes, 35 secondsAnd if one agent steps on another agent's toes, have them rectify by talking back and forth.
3:50:403 hours, 50 minutes, 40 secondsNow, I'm not going to fix this code base throughout the course of this video because that'll probably take several hours for it to go through, make changes.
3:50:473 hours, 50 minutes, 47 secondsAnd then obviously there's Q&A and testing and so on and so forth.
3:50:503 hours, 50 minutes, 50 secondsThis is pretty similar to the workflow that the creator of this godforsaken repo.
3:50:543 hours, 50 minutes, 54 secondsAnd if you're curious about why I have strong opinions on this, just check my channel for one of my videos from like two weeks ago or so.
3:51:013 hours, 51 minutes, 1 secondBut this is pretty similar to the workflow that they're currently using in order to manage things.
3:51:053 hours, 51 minutes, 5 secondsBut suffice to say, you can use an approach like this on basically any open source library, not just to identify security issues, but also to improve the product.
3:51:123 hours, 51 minutes, 12 secondsYou could come up with different product angles.
3:51:153 hours, 51 minutes, 15 secondsSo you could have it go through, spawn five agents, each that proposed different product ideas.
3:51:203 hours, 51 minutes, 20 secondsAnd then you could have like debate agents that debate back and forth about why this would not be a good idea.
3:51:243 hours, 51 minutes, 24 secondsAnd in doing so, they come to a consensus and improve the quality of the product at the end of the day.
3:51:293 hours, 51 minutes, 29 secondsSo what this just did is it spun up and listed all of the products here so that it could build a simple and straightforward way of basically splitting this work up.
3:51:383 hours, 51 minutes, 38 secondsSo now we have 10 scanner agents that are running in parallel across the code base. This first one here has 83K lines. The second one has 43, 42, 42, 35, 49.
3:51:473 hours, 51 minutes, 47 secondsSo they're not all the same obviously, but lines aren't all equal.
3:51:503 hours, 51 minutes, 50 secondsSo maybe this did some additional work behind the scenes or under the hood. Now this is gonna consume a lot of tokens. So see here, we're already at 1.3 million and counting.
3:51:593 hours, 51 minutes, 59 secondsAnd I'm consuming a fair amount of my own token budget to do this, but I figured it would just be interesting for us to do.
3:52:043 hours, 52 minutes, 4 secondsIf you're operating at like a massive scale like this with dozens, if not hundreds of these things, you will eventually spend several thousand dollars on said tokens.
3:52:123 hours, 52 minutes, 12 secondsAnd so you need to be prepared for that.
3:52:133 hours, 52 minutes, 13 secondsDon't spin up an unlimited number of agents if you're not capable of paying the money for upset unlimited number of agents, obviously.
3:52:203 hours, 52 minutes, 20 secondsAnd be wary that the tokens that you are using here are tokens that unfortunately you will never get back.
3:52:253 hours, 52 minutes, 25 secondsYou can't spin this thing up and then ask for a refund on any of these. So be careful, I guess, to make a long story short.
3:52:313 hours, 52 minutes, 31 secondsHowever, if you do have the money, you can basically convert it and translate it directly into time, as in time savings.
3:52:373 hours, 52 minutes, 37 secondsWhat I've done here is I've taken $1.3 million, $1.3 million, 1.3 million Sonnet 4.5 tokens.
3:52:433 hours, 52 minutes, 43 secondsAnd I basically immediately translated them for probably several hours of my time because I don't actually have to go through this or do this a lot slower with like a more intelligent agent.
3:52:513 hours, 52 minutes, 51 secondsSo now that we've done this, the next step is we've compiled all of these security possibilities, I should say.
3:52:583 hours, 52 minutes, 58 secondsWe're now spawning specific agents all about particular security issues.
3:53:053 hours, 53 minutes, 5 secondsSo we have a command injection and code execution agent, an authentication and authorization agent, a path traversal plus SSRF plus info disclosure agent,
3:53:143 hours, 53 minutes, 14 secondscrypto plus race conditions plus config agent, challenges findings as not real issues is devil number one, that's devil's advocate.
3:53:213 hours, 53 minutes, 21 secondsThen devil number two says, argues findings are real and need fixing.
3:53:243 hours, 53 minutes, 24 secondsAnd so basically these agents are all gonna report over to these puppies.
3:53:273 hours, 53 minutes, 27 secondsAnd these two are gonna debate back and forth between each other to determine whether or not this is something that's real, whether or not this is something that's actually super important.
3:53:353 hours, 53 minutes, 35 secondsThey're gonna take different principled positions and attempt to see whether these findings are not real issues or whether these findings are real and need fixing.
3:53:423 hours, 53 minutes, 42 secondsUsually when you have two agents work adversarially against each other like this, the end result is higher quality.
3:53:473 hours, 53 minutes, 47 secondsThis is actually the core of a big chunk of machine learning, which is what AI used to be called a few years back, not just large language models, generative adversarial networks
3:53:553 hours, 53 minutes, 55 secondswere one of the first image models, for instance, and they worked in a very similar fashion.
3:53:583 hours, 53 minutes, 58 secondsYou had something that generated and then you had sort of like an adversary.
3:54:013 hours, 54 minutes, 1 secondAnd these two just went back and forth and back and forth until you got a really high quality result.
3:54:053 hours, 54 minutes, 5 secondsSo I can actually scroll down here and see what these two are saying. So these are literally having a conversation right now. These are actually discussing things.
3:54:123 hours, 54 minutes, 12 secondsLooking at devil number two, it's sending round one, it's counter arguments now, we're going back here.
3:54:163 hours, 54 minutes, 16 secondsAnd they're basically like fighting verbally mind you, sticks and stones may break my bones, but AI words will hurt you, to determine which is the best path forward.
3:54:263 hours, 54 minutes, 26 secondsThis message over here, this wasn't mine. This was a message from the team lead.
3:54:303 hours, 54 minutes, 30 secondsBasically a devil number one responded to team lead saying, Hey, here's a key finding.
3:54:353 hours, 54 minutes, 35 secondsTeam leader responded back saying, Hey, keep going and let me know when the debate finishes. When it's done, we're good to go.
3:54:403 hours, 54 minutes, 40 secondsSo to be clear here over the last, I don't know, maybe 15 minutes or so of the specific agent team instantiation,
3:54:473 hours, 54 minutes, 47 secondsI've spent close to probably 80 or so dollars directly on this one query. And that's what I mean by trading money for time.
3:54:553 hours, 54 minutes, 55 secondsI mean, like, obviously, if I had a team of developers doing this, they probably would have been more accurate, but it also would have taken them presumably several weeks
3:55:023 hours, 55 minutes, 2 secondsto do the level of research that this agent was capable of doing in just a few minutes. I traded $80 for that time.
3:55:093 hours, 55 minutes, 9 secondsAnd so in some instances, that's worth it, but in a lot of other instances, it isn't, which is why agent teams need to be handled pretty carefully.
3:55:163 hours, 55 minutes, 16 secondsThey're almost like a nuclear weapon, just one aimed directly at your wallet. Now we've done the debate back and forth. The two have had a great conversation. And so basically they found 15 total flaws.
3:55:253 hours, 55 minutes, 25 secondsWhat it's doing is it's now spawning 15 fixer agents, one per isolated security issue or group when they touch the same file. Now I don't obviously want this to consume all of my tokens.
3:55:343 hours, 55 minutes, 34 secondsSo I'm just going to cancel this and I'll say shut in caps, shut everything down ASAP. It's not gonna shut down all active agents immediately.
3:55:433 hours, 55 minutes, 43 secondsUnfortunately, just due to the nature of this, the shutdown request doesn't occur immediately.
3:55:463 hours, 55 minutes, 46 secondsIt's not just like we're, you know, alt F foring this whole puppy.
3:55:493 hours, 55 minutes, 49 secondsThere's a little bit of time because every individual agent is still in the middle of a query while it's doing the thing.
3:55:553 hours, 55 minutes, 55 secondsSo, you know, you're probably still gonna consume a little bit of token usage. Not gonna be that crazy, but it is gonna be a little bit.
3:56:003 hours, 56 minutesBut yeah, you know, I've consumed enough at this point to know that this is something that I'm probably not gonna wanna do unless I'm hell bent on improving the OpenClaw repo, which I am not.
3:56:083 hours, 56 minutes, 8 secondsAnd that takes us to the final module in this course, which is one on Git work trees.
3:56:123 hours, 56 minutes, 12 secondsNow, Git work trees used to basically be what agent teams are today.
3:56:173 hours, 56 minutes, 17 secondsEssentially, you could have multiple agents all running on their own individual, what's called GitHub repo or GitHub branch.
3:56:233 hours, 56 minutes, 23 secondsAnd in doing so, these agents could all work on individual features, which allowed them to do what they needed to do before ultimately merging back to the main branch.
3:56:313 hours, 56 minutes, 31 secondsTo visualize this for you, imagine we start with a job over here at main. This is our main branch. And then there's some bugs.
3:56:383 hours, 56 minutes, 38 secondsWhat we do is we spin up a branch called hotfix. Now this is given to a different agent.
3:56:433 hours, 56 minutes, 43 secondsWe then have another branch called develop, which is given to a different agent.
3:56:463 hours, 56 minutes, 46 secondsThen finally, a branch called feature, which is given to a different agent.
3:56:493 hours, 56 minutes, 49 secondsSo basically what occurs is we have one agent over here, extending on this branch, one agent over here, extending on this branch, one agent over here, extending on this branch, and then one agent over here extending on this branch.
3:56:593 hours, 56 minutes, 59 secondsAnd essentially each of these go through their own development process, similar to the agent teams like we just saw here, just managed through GitHub instead of just the antigravity IDE.
3:57:073 hours, 57 minutes, 7 secondsAnd then when they're done, what they do is they merge the results back into the mage branch.
3:57:113 hours, 57 minutes, 11 secondsNow, if you're not a big into programming and you haven't used GitHub before, this idea of a merge can be pretty difficult to understand, but basically every branch just stores a copy of the folder.
3:57:203 hours, 57 minutes, 20 secondsAnd so this master folder is basically the same thing as this new feature folder with just a couple of minor differences. And it's usually the new feature itself.
3:57:283 hours, 57 minutes, 28 secondsSo when you merge, what you do is you're just tabulating a list of all the changes between these two, and then you're taking the new changes from the new feature branch and then applying them to the master branch.
3:57:373 hours, 57 minutes, 37 secondsThis merge process can typically be pretty messy.
3:57:403 hours, 57 minutes, 40 secondsAnd so having agents around to mediate the merges and so on and so forth can be quite useful.
3:57:443 hours, 57 minutes, 44 secondsSo first of all, I have a really simple website set up here called leftclick-agency.
3:57:483 hours, 57 minutes, 48 secondsI made this for my own website a while ago and had AI do the vast majority of the work in a very similar sort of workflow to what I just showed you guys a moment ago with agent teams.
3:57:583 hours, 57 minutes, 58 secondsAnd what I wanna do is, I wanna design additional pages here. One page that's really long is not enough.
3:58:043 hours, 58 minutes, 4 secondsSo in addition to this homepage, I also wanna design an about page, a contact page and a services page.
3:58:123 hours, 58 minutes, 12 secondsAnd I wanna use the Git WorkTree workflow in order to do this.
3:58:173 hours, 58 minutes, 17 secondsNow, because I've stored information on what I mean by Git WorkTree in my Claw.md, which is basically that we use Git WorkTrees
3:58:263 hours, 58 minutes, 26 secondsfor parallel development with Claude Code where every WorkTree is an isolated working directory, sharing the same Git history, allowing multiple Claude Code instances to work on different tasks simultaneously that interference, okay?
3:58:353 hours, 58 minutes, 35 secondsThis already knows what to do.
3:58:373 hours, 58 minutes, 37 secondsThe very first thing that it did was it basically took my main repository, which was just called leftclick-agency, and it made three different ones.
3:58:473 hours, 58 minutes, 47 secondsIt made one called leftclick-agency-services, that's a new folder.
3:58:513 hours, 58 minutes, 51 secondsAnother called leftclick-agency-about, which is another folder.
3:58:553 hours, 58 minutes, 55 secondsAnd then another called leftclick-agency-contact, which is a third folder.
3:58:593 hours, 58 minutes, 59 secondsSo basically what it's doing now is it's creating new GitHub repositories, new individual feature branches to work on different pages for me.
3:59:073 hours, 59 minutes, 7 secondsNow it did this using the agent team functionality.
3:59:103 hours, 59 minutes, 10 secondsAnd the reason why I did this is just because it's much faster and they get to work on different GitHub repos simultaneously.
3:59:153 hours, 59 minutes, 15 secondsThe only real advantage to using Git WorkTrees, I think at this point, is just that when you use Git WorkTrees, what you're doing is you're not actually modifying the main folder.
3:59:233 hours, 59 minutes, 23 secondsLike if you look up here, we're not actually modifying any of this right now.
3:59:253 hours, 59 minutes, 25 secondsWhat we're doing is we're basically creating a new folder in our GitHub repository. And you can see them right over here.
3:59:333 hours, 59 minutes, 33 secondsAnd then working in those different folders individually. So for instance, we have leftclick-agency. This one here is my main folder, right?
3:59:413 hours, 59 minutes, 41 secondsBut then we have leftclick-agency-about.
3:59:433 hours, 59 minutes, 43 secondsThis is a new branch that's working specifically on the about. Then we have contact.
3:59:473 hours, 59 minutes, 47 secondsThis is a new branch working specifically on the contact page.
3:59:503 hours, 59 minutes, 50 secondsAnd then finally we have services, which is a new branch working specifically on the services page.
3:59:543 hours, 59 minutes, 54 secondsAnd so the reason why this is valuable to people that are non-programmers is because you reduce the possibility of two different agents working on the same file, which will occur.
4:00:044 hours, 4 secondsYou will get what are called agent conflicts over time naturally as you have multiple agents working on multiple things in the same base.
4:00:114 hours, 11 secondsAnd the reason why is because files aren't perfect separations of functionality.
4:00:144 hours, 14 secondsYou know, you'll have one file and then that file will have like the snippet of a little bit of code that's used by another file and so in that way, there's never like perfect separation.
4:00:224 hours, 22 secondsSo if an agent really wants to like totally encapsulate a function or something, sometimes it'll have to dump around from both.
4:00:284 hours, 28 secondsAnd when that happens, it'll step on the toes of another one.
4:00:304 hours, 30 secondsSo anyway, when you use work trees in this way, you just eliminate that for being an option completely.
4:00:354 hours, 35 secondsBasically, there's just no way that these two can step on each other's toes because they're actually all working in separate folders simultaneously.
4:00:414 hours, 41 secondsAnd because they work in separate folders, that also means if they do make changes, those changes don't always perfectly harmonize right away. And that's where that merge step comes into play.
4:00:494 hours, 49 secondsBasically, you know, now after we work on these three different branches, we also have to do is we need to unify them through some sort of merge function.
4:00:594 hours, 59 secondsYou can see the prompt for the general purpose services.html one here is you're building the services.html page for left click agency, work in the work tree at this folder, create the file in here, don't modify the index.html.
4:01:124 hours, 1 minute, 12 secondsYou know, there's tons of information.
4:01:134 hours, 1 minute, 13 secondsIf I go back here, we'll see the same thing for contact.html.
4:01:174 hours, 1 minute, 17 secondsAnd so all you really need in order to have a workflow like this that minimizes dependency risks, is just have a cloud.md that outlines what to do with get work trees.
4:01:284 hours, 1 minute, 28 secondsI'll include this file down below so you guys have everything that you need, but taking a look at the about.html here, which is one of the files that this thing just whipped up for me.
4:01:364 hours, 1 minute, 36 secondsYou know, we've now basically finished the page.
4:01:384 hours, 1 minute, 38 secondsIt's used some placeholder little flasks here because it wasn't sure what I wanted to do for images.
4:01:434 hours, 1 minute, 43 secondsAnd you know, I can add however much information I want here to really flesh it out. Yeah, this looks pretty clean to me. Our principal's ready to work with us.
4:01:504 hours, 1 minute, 50 secondsOoh, that's really clean. I didn't realize I could do that. Likewise with the contact.html page.
4:01:564 hours, 1 minute, 56 secondsSo now we have a beautiful contact.html page with like a little, send us a message form and so on and so forth. I wonder if that works. Wow, it even has some validation.
4:02:044 hours, 2 minutes, 4 secondsThat's pretty neat.
4:02:054 hours, 2 minutes, 5 secondsOkay, and then after it's done, what it'll do is it'll merge everything together.
4:02:084 hours, 2 minutes, 8 secondsSo we also have services.html up here too, which is really clean.
4:02:144 hours, 2 minutes, 14 secondsThis looks like a real chunky page, which probably explains why it took much longer.
4:02:174 hours, 2 minutes, 17 secondsAnd it looks like it even estimated some prices for them, which is pretty nice. So yeah, suffice to say, get work trees.
4:02:234 hours, 2 minutes, 23 secondsWhile not necessarily the end all be all, can be an extra layer of insulation if you guys are using something like, you know, agent teams, or even if you guys are using sub agents,
4:02:324 hours, 2 minutes, 32 secondsjust using some sort of merge functionality like I talked about.
Chapter 34: Utilizing Git Worktrees
4:02:354 hours, 2 minutes, 35 secondsIf you guys want more on that, I'll include the cloud.md below so you guys have everything that you need.
4:02:394 hours, 2 minutes, 39 secondsOkay, so we've talked a lot about using Claude Code to build cool software apps and stuff like that.
4:02:434 hours, 2 minutes, 43 secondsThe last thing I wanna talk about is basically just automating or significantly streamlining the process of deploying things to the internet.
4:02:494 hours, 2 minutes, 49 secondsYou guys remember that first site that I made and then the proposal generator and stuff like that?
4:02:534 hours, 2 minutes, 53 secondsI use a simple service called Netlify to basically push my work live. And that service works really well for static sites.
4:02:584 hours, 2 minutes, 58 secondsI'm not affiliated with them whatsoever, by the way, use whatever the heck you want.
4:03:014 hours, 3 minutes, 1 secondBut what I wanna talk about next is using something analogous to that just for backend functions and skills and scripts and so on.
Chapter 35: Deploying APIs with Modal
4:03:094 hours, 3 minutes, 9 secondsSo what I'm gonna do here is I'm gonna whip up open conversation, I'm gonna jump into bypass permissions and I'm gonna say deploy a simple API endpoint
4:03:164 hours, 3 minutes, 16 secondsthat returns happy birthday Nick if it's my birthday or no birth to today MF if it's not.
4:03:214 hours, 3 minutes, 21 secondsAnd what I wanna do is I wanna show you guys how easy it is to basically whip up your own URL that does something for you.
4:03:284 hours, 3 minutes, 28 secondsThis is more of an advanced feature for people that are into automation and workflow building and stuff like that.
4:03:334 hours, 3 minutes, 33 secondsBut basically these services, in my case I'm using one called modal, allow you to whip up like publicly available endpoints
4:03:404 hours, 3 minutes, 40 secondsor publicly available URLs that you can use and integrate within other applications.
4:03:454 hours, 3 minutes, 45 secondsA lot of the time applications will use things called web hooks to send and receive information to and from them to send events and trigger various pieces of functionality.
4:03:544 hours, 3 minutes, 54 secondsAnd this is a quick and easy way that you can basically create a URL that does that for you, as well as integrate it into things like no code platforms like NADAN, make.com, Zapier, Lindy, et cetera.
4:04:034 hours, 4 minutes, 3 secondsSo I have my endpoint right over here.
4:04:054 hours, 4 minutes, 5 secondsWhat I'm gonna do is I'm gonna take this curl, which you may be wondering like what the heck's going on over here?
4:04:104 hours, 4 minutes, 10 secondsI'm actually gonna open up my own terminal instance and I'm gonna paste that in.
4:04:144 hours, 4 minutes, 14 secondsSo basically what I've done, to make a long story short, is I have generated my own API.
4:04:204 hours, 4 minutes, 20 secondsZooming in here, what I've done is I've sent a request to my own website, which was nicholasriov-birthday-check-check-birthday.modelrun.
4:04:304 hours, 4 minutes, 30 secondsAnd then I sent some authorization credentials and stuff like that.
4:04:334 hours, 4 minutes, 33 secondsAnd now it's sending me back a little message, which is basically saying, no birthday today, MF, because it's not my birthday.
4:04:414 hours, 4 minutes, 41 secondsSo what I'm gonna do is I'm gonna go back here and just to make it even clearer for you guys, okay, this is awesome. Remove the authentication.
4:04:464 hours, 4 minutes, 46 secondsI wanna be able to access this with my browser using a simple get, and then I wanna have like a cute little happy birthday or no birthday message.
4:04:534 hours, 4 minutes, 53 secondsWhat I'm gonna do now is I'm gonna show you that this is analogous or equivalent to just a website.
4:04:574 hours, 4 minutes, 57 secondsAnd so what you can do as well is you can basically take whatever you want, whatever piece of functionality, and then immediately deploy like single URLs that people can access to do things, which you may not think is super important.
4:05:074 hours, 5 minutes, 7 secondsI just opened this up and we have our own little website here, but as you saw there, I mean, all I did was I literally sent like one little message and then boom, it bumped this on, like made this a publicly accessible website.
4:05:174 hours, 5 minutes, 17 secondsYou can do this with anything.
4:05:184 hours, 5 minutes, 18 secondsYou can do this with the websites that we've designed so far, you can do this with the web apps that we've designed so far.
4:05:224 hours, 5 minutes, 22 secondsAnd it's just like the simplest and easiest way to get something web accessible, whether you are giving a URL to somebody to have them do something with, creating some additional functionality in your app,
4:05:314 hours, 5 minutes, 31 secondslogging user visits for things like ad campaigns and marketing campaigns, or doing direct connections via web hooks and no code platforms like make.com, and add in, et cetera.
4:05:414 hours, 5 minutes, 41 secondsSo how do you do it? My favorite service right now is one called modal.
4:05:434 hours, 5 minutes, 43 secondsThis is basically marketed as AI infrastructure that developers love. It's super easy and straightforward to set up.
4:05:494 hours, 5 minutes, 49 secondsAnd every time you click on the page, it expands this damn square thing, which is super, super cool to look at. I love cubes.
4:05:564 hours, 5 minutes, 56 secondsClearly their team has spent a lot of time in ownership designing this website. I wonder if they use Claude Code. Anyway, what you have to do first, you have to sign up. So I'm just gonna go over here into an incognito tab.
4:06:054 hours, 6 minutes, 5 secondsI'm gonna pretend that I don't have an account yet. Then I'm going to click sign up.
4:06:084 hours, 6 minutes, 8 secondsThen I'm going to continue with, why don't we continue with Google? Then I'm just gonna sign in. Cool, we are now signing in.
4:06:144 hours, 6 minutes, 14 secondsAnd the very first thing that happens is you'll have some little onboarding screen that says welcome to modal. So I'm just gonna say personal. And how did we hear about us? Social media, I don't know.
4:06:224 hours, 6 minutes, 22 secondsI just wanna use this for other. Then I'll click get started.
4:06:264 hours, 6 minutes, 26 secondsWhat it's going to do now is it's gonna give me access to all sorts of stuff.
4:06:294 hours, 6 minutes, 29 secondsAnd in the top right hand corner, as you see, it's given us $5 in credits.
4:06:334 hours, 6 minutes, 33 secondsYou can actually claim up to $30 in credits just by doing a few additional little onboarding tasks, adding a card and stuff like that.
4:06:394 hours, 6 minutes, 39 secondsI should note that I've been using modal for quite a while now. It's probably been a few months.
4:06:424 hours, 6 minutes, 42 secondsAnd I think I'm still at $4.50 of credit on my main account where we probably have an API request coming in every day or two.
4:06:504 hours, 6 minutes, 50 secondsSo yeah, pretty cool stuff. Definitely a lot of usage there with the $5.
4:06:544 hours, 6 minutes, 54 secondsIt's way cheaper of a service, I wanna say, than a lot of the no-code tools and automation platforms that I was using before like make.com and N8n.
4:07:024 hours, 7 minutes, 2 secondsOver here, what you need to do is create an API token. So I'm gonna click new token. I'll say for, what did I actually have over here? That was pretty interesting. Not genuine.
4:07:114 hours, 7 minutes, 11 secondsOkay, let's do that. And then we actually have the token. So I'm just gonna copy the server. And then what I wanna do is I just wanna paste this on.
4:07:194 hours, 7 minutes, 19 secondsAnd in the CLAW.md, there's instructions where basically you can just give it a new token and then it'll go and create all of the stuff for you.
4:07:264 hours, 7 minutes, 26 secondsSo I'm gonna include this in the description down below. You guys can take a peek at this.
4:07:304 hours, 7 minutes, 30 secondsIf you're new to this, all you have to do is just do what I just showed you.
4:07:334 hours, 7 minutes, 33 secondsAnd then now you have the ability to basically run this on any account.
4:07:374 hours, 7 minutes, 37 secondsAnd you know this because this new URL that I just popped up here, this is on a different service now.
4:07:424 hours, 7 minutes, 42 secondsIt's on my Nick J. Wells account, not my Nicholas Sariath account, which I was on just a moment ago. But let's say you wanna extend this.
4:07:474 hours, 7 minutes, 47 secondsYou don't just wanna do a simple URL that, I don't know, like tells you whether or not it's your birthday. You actually wanna do something for business purposes. Well, here's where things get really interesting.
4:07:564 hours, 7 minutes, 56 secondsWhat you can do is you can take a skill that you've developed before. Then you can just put it up on a URL.
4:08:014 hours, 8 minutes, 1 secondSo that every time you or somebody else accesses URL, it immediately triggers the workflow. Let me show you what I mean.
4:08:074 hours, 8 minutes, 7 secondsRemember how when we chatted about skills, I have this one called scrape leads?
4:08:104 hours, 8 minutes, 10 secondsWhat if I just copy this and then paste this directly into this folder?
4:08:154 hours, 8 minutes, 15 secondsI'm also going to wrap it in a .clod and then a skills folder just for organization sake, because I could tell this is probably gonna get pretty complex if I don't.
4:08:234 hours, 8 minutes, 23 secondsOkay, and now I have it inside of .clod slash skills slash scrape dash leads.
4:08:264 hours, 8 minutes, 26 secondsNow what I'm gonna do is I'm gonna say, this is great.
4:08:284 hours, 8 minutes, 28 secondsWhat I'd like you to do now is I'd like you to put the scrape dash leads workflow online. I want to be able to access it via a simple URL.
4:08:364 hours, 8 minutes, 36 secondsBasically, when I access scrape dash leads, I want a little form to pop up and ask me what I wanna scrape.
4:08:424 hours, 8 minutes, 42 secondsI then fill out that form and then you execute the scrape dash leads workflow and then return me the leads in a CSV file when it's done.
4:08:504 hours, 8 minutes, 50 secondsAnd this has taken us probably less than two minutes in total. I just filled out the request. We now have a URL.
4:08:564 hours, 8 minutes, 56 secondsJust gonna open up this URL, which is as mentioned the same as any other URL. The search query I'm gonna do is I'll just do dentist. I'll say United States.
4:09:044 hours, 9 minutes, 4 secondsWe want, I don't know, 100 results. Let's make it really small.
4:09:074 hours, 9 minutes, 7 secondsNow that we've clicked, we're just gonna take a few minutes to do the actual scrape.
4:09:114 hours, 9 minutes, 11 secondsThe instructions I gave it were to immediately download the CSV right as this is done.
4:09:154 hours, 9 minutes, 15 secondsOkay, and then top right hand corner, I have my leads underscore dentist 100.
4:09:184 hours, 9 minutes, 18 secondsSo I'm just gonna take a peek at this and we have the data right over here. Okay, looking pretty good. We have 100 leads. Most of these look like dentists, if not all.
4:09:264 hours, 9 minutes, 26 secondsWe also have a bunch of additional data about them, which is pretty bad ass. So I could use this to build a really cool campaign.
4:09:314 hours, 9 minutes, 31 secondsAnd yeah, hopefully you guys now see the power in having something as simple as modal available to both whip up really quick webpages and internal tooling
4:09:394 hours, 9 minutes, 39 secondsand even some external tooling, as well as use this to do things like run workflows, right?
4:09:444 hours, 9 minutes, 44 secondsYou can build your own API call or build your own API endpoint, I should say, really just a couple of keystrokes. And that's that.
4:09:504 hours, 9 minutes, 50 secondsI hope you guys enjoyed learning everything and anything to do with Claude Code today.
4:09:544 hours, 9 minutes, 54 secondsYou now have everything that you need to build the foundational base of knowledge, whether or not you guys are programmers or completely non-technical people coming into this
4:10:024 hours, 10 minutes, 2 secondsto learn how to do things like build simple apps, websites, or workflows. I had a blast teaching you guys this sort of stuff.
4:10:084 hours, 10 minutes, 8 secondsIf you've ever wondered how to monetize work like this, whether it is custom app development or workflow building, I'd definitely check out Maker School.
4:10:154 hours, 10 minutes, 15 secondsIt's my 90-day accountability program where I guide you through step-by-step and quite literally every single day through a sequence of actions necessary to get you your very first customer.
4:10:244 hours, 10 minutes, 24 secondsAnd I also guarantee that you get your first customer by the end of a 90-day period. If you don't, I give you all your money back. That's my last and only pitch of this video.
4:10:324 hours, 10 minutes, 32 secondsAside from that, I hope you guys like what you saw.
4:10:344 hours, 10 minutes, 34 secondsIf you guys have any questions or need help with anything that I mentioned in the video, just drop it as a comment down below.
4:10:394 hours, 10 minutes, 39 secondsAside from that, have a lovely rest of the day, and I'll catch all y'all in my next course. Bye.

Sync to video time
