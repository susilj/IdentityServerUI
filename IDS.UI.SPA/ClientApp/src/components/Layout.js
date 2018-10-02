import React from 'react';
import { Link } from 'react-router-dom';
//import { Col, Grid, Row } from 'react-bootstrap';
//import NavMenu from './NavMenu';
import { Layout, Row, Col, Icon, Menu } from 'antd';

//export default props => (
//  <Grid fluid>
//    <Row>
//      <Col sm={3}>
//        <NavMenu />
//      </Col>
//      <Col sm={9}>
//        {props.children}
//      </Col>
//    </Row>
//  </Grid>
//);

const { Header, Content } = Layout;

export default props => (
  <Layout>
    <Header style={{ background: '#fff', padding: 0 }}>
      <Menu
        mode="horizontal"
        defaultSelectedKeys={['2']}
        style={{ lineHeight: '64px' }}
      >
        <Menu.Item key="1">User</Menu.Item>
        <Menu.Item key="2"><Link to={'/roles'}>Roles</Link></Menu.Item>
        <Menu.Item key="3">Claim Types</Menu.Item>
      </Menu>
    </Header>
    <Content>
      {props.children}
    </Content>
  </Layout>
);
