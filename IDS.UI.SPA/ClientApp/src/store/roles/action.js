import axios from 'axios';

export const fetchRoles = () => {
    return (dispatch) => {
        dispatch({ type: 'FETCH_ROLE' });
        //'Authorization': `Bearer ${localStorage.getItem('token')}`
        return axios.get('api/role/', { headers: {  } }).then(response => {
            dispatch({ type: 'FETCH_ROLE_SUCCESS', payload: response.data });
        }).catch(error => {
            dispatch({ type: 'FETCH_ROLE_FAILURE', payload: error });
        });
    }
}