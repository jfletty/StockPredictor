#!/usr/bin/env python

import pandas
import sys
import datetime
import logging
from sklearn.ensemble import RandomForestRegressor
import storage
import prepare

from joblib import Parallel, delayed
import multiprocessing

container = storage.Container
predictAsAt = datetime.datetime.utcnow().replace(tzinfo=datetime.timezone.utc).isoformat()

def main(predictAsAt):
    predict(sys.argv[1])

def predict(itemName):

    try:
        print("Downloading Data")
        (df, stockKey) = storage.get_projection_input_content(container.projection_input, itemName)
        if isinstance(df, pandas.DataFrame):
            X = prepare.input_df_x(df)
        
            print("Downloading model")
            clf = storage.get_model(stockKey)
        
            if not isinstance(clf, type(None)):
                Y = clf.predict(X)
                df[['RegularMarketClose']] = Y
                print("Saving prediction")
        
                storage.save_projection(stockKey, df)

            storage.cleanup_inputs(container.projection_input, itemName)
        print("Successfully Completed Prediction. " + itemName)

    except Exception as e:
        print (e)
        print("Moving blob to exception " + itemName)

main(predictAsAt)