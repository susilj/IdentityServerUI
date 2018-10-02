export default function reducer(state = {
    roles: [],
    loading: false,
    error: null
}, action) {
  switch (action.type) {
    case 'FETCH_ROLE':
      return { ...state, loading: true };
    case 'FETCH_ROLE_SUCCESS':
      return { ...state, roles: action.payload, loading: false };
    case 'FETCH_ROLE_FAILURE':
      return { ...state, error: action.payload, loading: false };
    default:
      return { ...state };
  }
}