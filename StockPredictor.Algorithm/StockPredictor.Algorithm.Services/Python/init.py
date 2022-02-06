from subprocess import STDOUT, check_call
import os

check_call(['apt-get', 'update'],
     stdout=open(os.devnull,'wb'), stderr=STDOUT) 

check_call(['apt-get', 'install', '-y', 'python3-pip'],
     stdout=open(os.devnull,'wb'), stderr=STDOUT) 
check_call(['sudo', 'pip3', 'install', '-U', 'scikit-learn'],
     stdout=open(os.devnull,'wb'), stderr=STDOUT) 
check_call(['sudo', 'pip3', 'install', '-U', 'pandas'],
     stdout=open(os.devnull,'wb'), stderr=STDOUT) 
check_call(['sudo', 'pip3', 'install', '-U', 'boto3'],
     stdout=open(os.devnull,'wb'), stderr=STDOUT) 