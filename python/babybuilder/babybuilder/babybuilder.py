import string
import sys
import os
from datetime import *
import time
import xml.dom.minidom
import threading 
import glob
import subprocess
import distutils
import ctypes
import Queue
import shutil
import zipfile
import stat
import binascii
from xml.dom.minidom import Document
import smtplib


env = os.environ.copy()
localbuild = True 



#def runcommandsync(cmd, loglevel = "BUILDINFO", shell = True, env = None):





def BuildWebPlayer(args=None):
    
    global env 

    if ( not localbuild ):
        deployfolder = env['DEPLOYFOLDER'] 
    else :
        deployfolder = "d:/BABYBUIDER"

    os.chdir("C:/Program Files (x86)/Unity/Editor") 
    
    outlog (os.getcwd()) 

    cmd =  "Unity.exe -quit -batchmode -nographics -buildWebPlayer %s -logFile %s/buildWebPlayer.txt "% (deployfolder,deployfolder)

    outlog ("About to run command: " + str(cmd))
    
    
    
    bs=subprocess.check_call( cmd  )# , env = env, shell = shell, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
    




def InitBuild(args=None):
    env = os.environ.copy()
    for s in env :
        outlog  ("%s =  %s "% (s, env[s]) )
    return


def notifybuild (args=None):

    global env
    from email.mime.multipart import MIMEMultipart
    from email.mime.text import MIMEText


    # Create message container - the correct MIME type is multipart/alternative.
    msg = MIMEMultipart('alternative')
    msg['Subject'] = r"babybuilder notification" 
    msg['From'] = r"babybuildmaster@gmail.com" 
    msg['To'] = r"shanghalf1967@gmail.com"
   
    fn = "D:/BABYBUIDER/buildWebPlayer.txt"
    filehandle= open(fn,"r")
    lines = filehandle.readlines()
    filehandle.close()


    # Create the body of the message (a plain-text and an HTML version).
    text = "buid notification"
    
    # Record the MIME types of both parts - text/plain and text/html.
    part1 = MIMEText(lines, 'plain')
    part2 = MIMEText(lines, 'html')

    # Attach parts into message container.
    # According to RFC 2046, the last part of a multipart message, in this case
    # the HTML message, is best and preferred.
    msg.attach(part1)
    msg.attach(part2)

    # Send the message via local SMTP server.
    #s = smtplib.SMTP('smtp.free.fr:587')

     
    s = smtplib.SMTP_SSL("smtp.mail.yahoo.com",timeout=10)
    
    hello = s.ehlo() 
    outlog  ( hello  ) 

    s.login(r"babybuildmaster@yahoo.com",r"wadamadafaka")


    # sendmail function takes 3 arguments: sender's address, recipient's address
    # and message to send - here it is sent as one string.

    s.sendmail(r"babybuildmaster@yahoo.com", r"shanghalf1967@gmail.com",msg.as_string())

    s.quit()
    # display the link of the full log 
    outlog ( "Build notification sent : content file : %s"% "buildWebPlayer.txt")


    return 



def step4(args=None):
    outlog("step4")
    return





def BuildProject(step=None):

    global projectFolder
    global logpath
    
    print"      _           _ _     _                                          "
    print"     | |         (_) |   | |                                         "
    print"     | |__  _   _ _| | __| |___  ___  __ _ _   _  ___ _ __   ___ ___ "
    print"     | '_ \| | | | | |/ _` / __|/ _ \/ _` | | | |/ _ \ '_ \ / __/ _ \\"
    print"     | |_) | |_| | | | (_| \__ \  __/ (_| | |_| |  __/ | | | (_|  __/"
    print"     |_.__/ \__,_|_|_|\__,_|___/\___|\__, |\__,_|\___|_| |_|\___\___|"
    print"                                        | |                          "
    print"                                        |_|                          "

    '''
     build sequence :
     this sequence could be the entry point for a build sequencer 
     aht would allow user to add custom build steps 
     sequence array take both command line to delegate the build to the masternode ( original folder of the unity project )
     functions could be local ( this file ) or executed in a subprocess or mix of both 
     required API are p4 and jenkins <todo>
    '''
    cmdbuff=[]
    
    #cmdbuff.append( "MapDescAutoGenerater.GenerateMapDesc" )
    cmdbuff.append([InitBuild])
    cmdbuff.append([BuildWebPlayer])
    cmdbuff.append([notifybuild])
    cmdbuff.append([step4])


    outlog ("================================= buildsequence")
    for n in cmdbuff :
        if type(n) is str :
            outlog (n )
        else:
            outlog( str(n))
    outlog ("================================= buildsequence")
    


    # run the sequence 
    for command in cmdbuff :
        if type(command) is str :
            logfilepath = logpath+"/"+command +".log"
            binf  = Buildinfos(projectFolder+selectedbranch+"/Unity",command,logfilepath,command)
            outlog("%s start"% (command))
            BuildThread  = UThread(binf)
            BuildThread.start()
            threading.Thread.join(BuildThread)

            #--------------------------------------------------------------------- CHECK BUILD RESULT 

            if os.path.exists( binf.getblogfilepath()): # does node got a log ?
                    # process returned info >> LOG 
                    lfhstr= binf.getblogfilepath() #curtbuildlogfname
                    if os.path.exists(lfhstr):
                        lfh = open( lfhstr , "r" ) 
                    else :
                        failbuild("BUILD FAIL : cannot find log path %s"% binf.getblogfilepath()) 
                                       
                    outlog ( copylogtoserver(binf.getblogfilepath()))

                    b_success = False 
                    sucess = False 
                    for l in lfh :
                        #outlog ("check result : >> %s "%l)
                        #masterloghndl.write(l)
                        b_success = l.find('Exiting batchmode successfully now!')
                        if b_success > -1 :
                            tmpstr = 'Sucess ----> ' + os.path.basename( binf.getblogfilepath() )
                            sucess = True
                            outlog (tmpstr )
                            break 
                    lfh.close()
            if(not sucess):
                failbuild("BUILD FAIL ON %s"%command)
                

            #--------------------------------------------------------------------- CHECK BUILD RESULT 
            outlog("%s finished"% (command))
        else :
            cmdtarget = command[0]
            command.remove(command[0])
            cmdargs = command
            cmdtarget(cmdargs)




def outlog(str="",thiseventlevel="BUILDINFO"):
    global localbuild
    global logpath 
    if (str == None ):
        return
    ts = datetime.fromtimestamp(time.time()).strftime('%H:%M:%S')
    logstring =  ("[%s]--> %s"%(thiseventlevel,str))
    # print out in console log 
    print logstring
    # push on global log anyway 

    if ( os.path.exists ( os.path.join(logpath,"full_log.txt") )):
        fh = open ( os.path.join(logpath,"full_log.txt"), "a" )
    else:
        fh = open ( os.path.join(logpath,"full_log.txt"), "w" )
    fh.write(logstring +'\n')    
    fh.flush()
    fh.close()
    return logstring



def runexternalcommand (cmd):

    outlog ("START BUILD .... OR NOT ","FLOOD")

    global projectFolder
    global logpath 


    outlog ("current folder : %s"%os.getcwd(),"FLOOD")
    outlog( "runexternalcommand projectFolder = %s"% projectFolder,"FLOOD")
    outlog ("runexternalcommand logpath = %s"% logpath,"FLOOD")
    outlog ("---- BUILD EXECUTED FROM JENKINS ----","FLOOD")
    localbuild= False


    # output arg table to console
    for n in cmd:
        if ( cmd.index(n) >0 ) : #bypass self ..
            outlog ("arg nb %s = %s"% ( cmd.index(n) , n) )
    outlog( "start build >>>>> ")
    # build from a jenkins job overide projectfolder
    # define action prior to build anything 
    if "buildproject" in cmd:
        outlog ("BUILD PROJECT STEP STARTED")
        BuildProject() 
    outlog ("end of the execution")
    return

#------------------------------------------------------------ ROOT SCOPE INIT AND RUN

localbuild = True 
projectFolder = os.getcwd() +'/'# current directory as build base 
logpath = projectFolder+"log/"
if ( not os.path.exists(logpath) ):
    os.mkdir (logpath )
outlog ("ENTRY POINT","FLOOD")
localbuild = True
runexternalcommand (sys.argv)




