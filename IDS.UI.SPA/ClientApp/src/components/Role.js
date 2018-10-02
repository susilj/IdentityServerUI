import React, { Component, Fragment } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { Row, Col } from 'antd';

import { fetchRoles } from '../store/roles/action';

class Role extends Component {
  render() {
    return (
      <Row>
        <Col>
          <span>Roles</span>
        </Col>
      </Row>
    );
  }
}

export default connect(
    state => state.roles,
    dispatch => bindActionCreators(fetchRoles, dispatch)
)(Role);