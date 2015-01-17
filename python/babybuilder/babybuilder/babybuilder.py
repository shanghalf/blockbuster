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
import ftplib


env = os.environ.copy()
localbuild = True 

def upload(ftp, file):
    ext = os.path.splitext(file)[1]
    if ext in (".txt", ".htm", ".html"):
        ftp.storlines("STOR %s"%os.path.basename(file), open(file))
    else:
        ftp.storbinary("STOR %s"%os.path.basename(file), open(file, "rb"), 1024)


def deploy (args=None):
    ftp = ftplib.FTP("ftpperso.free.fr")
    ftp.login("alfman", "fghpgxns")
    upload(ftp,os.path.normpath(u"D:/BABYBUILDER/BABYBUILDER.unity3d"))
    upload(ftp, os.path.normpath(u"D:/BABYBUILDER/BABYBUILDER.html"))
    upload(ftp, os.path.normpath(u"D:/BABYBUILDER/buildWebPlayer.txt"))



def BuildWebPlayer(args=None):
    
    global env 

    if ( not localbuild ):
        deployfolder = env['DEPLOYFOLDER'] 
    else :
        deployfolder = "d:/BABYBUILDER"

    os.chdir("C:/Program Files (x86)/Unity/Editor") 
    outlog (os.getcwd()) 
    cmd =  r"Unity.exe -quit -batchmode -nographics -projectPath E:/jenkins/jobs/test/workspace -buildWebPlayer %s -logFile %s/buildWebPlayer.txt "% (deployfolder,deployfolder)
    outlog ("About to run command: " + str(cmd))
    bs=subprocess.check_call( cmd  )# , env = env, shell = shell, stdout = subprocess.PIPE, stderr = subprocess.PIPE)
    




def InitBuild(args=None):
    env = os.environ.copy()
    for s in env :
        outlog  ("%s =  %s "% (s, env[s]) )
    return


def notifybuild (args=None):

    global env

    fn = "D:/BABYBUILDER/buildWebPlayer.txt"
    filehandle= open(fn,"r")
    lines = filehandle.readlines()
    filehandle.close()
    buildsucess =  False 
    readcounter =0
    while readcounter  < len(lines) -1  :
        n = lines[readcounter]
        readcounter +=1
        if ( n.find('Exiting batchmode successfully now!') > -1) :
            buildsucess =  True
            break
     
    s = smtplib.SMTP_SSL("smtp.mail.yahoo.com",timeout=100)
    hello = s.ehlo() 
    outlog  ( hello  ) 
    s.login(r"babybuildmaster@yahoo.com",r"wadamadafaka")
    if ( buildsucess):
        s.sendmail(r"babybuildmaster@yahoo.com", r"shanghalf1967@gmail.com",r"build %s SUCESS cl %s "% ( env['BUILD_NUMBER'] , env['GIT_COMMIT']  ))
    else :
        s.sendmail(r"babybuildmaster@yahoo.com", r"shanghalf1967@gmail.com",r"build %s fail cl %s"% ( env['BUILD_NUMBER'] , env['GIT_COMMIT']  ))

    s.quit()
    # display the link of the full log 
    outlog ( "Build notification sent : content file : %s"% "buildWebPlayer.txt")


    return 



def step4(args=None):
    outlog("step4")
    return

def failbuild( error ):
    outlog ( error )





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

    cmdbuff=[]
    cmdbuff.append([InitBuild])
    cmdbuff.append([BuildWebPlayer])
    cmdbuff.append([deploy])
    cmdbuff.append([notifybuild])


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




