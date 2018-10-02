import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import Counter from './components/Counter';
import FetchData from './components/FetchData';
import Role from './components/Role';

export default () => (
  <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/counter' component={Counter} />
    <Route path='/roles' component={Role} />
    <Route path='/fetchdata/:startDateIndex?' component={FetchData} />
  </Layout>
);
