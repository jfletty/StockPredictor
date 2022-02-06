#!/usr/bin/env python

import sys
import importlib

import pandas

import datetime
import logging

from sklearn.ensemble import RandomForestRegressor
import storageLocal as storage

import prepare

from joblib import Parallel, delayed
import multiprocessing

container = storage.Container

def main():
    print("Beginning Training")
    
    train_model(sys.argv[1])

    print("Finished Training")

def train_model(itemName):
    try:

        print("Downloading Data")
        
        (df, name) = storage.get_model_input_content(container.model_input, itemName)
        
        if isinstance(df, pandas.DataFrame):
            X = prepare.input_df_x(df)
            Y = df[['RegularMarketClose']]
            print("Training model")
            clf = RandomForestRegressor(n_estimators=100)
            clf.fit(X, Y.values.ravel()) # not sure about '.values.ravel()' but it removes the error
        
            print("Saving model")
        
            storage.save_model(name, clf)
            storage.cleanup_inputs(container.model_input, itemName)
        print("Successfully Completed Training. " + itemName)

    except:

        print("Moving blob to exception " + itemName)
        raise


main()