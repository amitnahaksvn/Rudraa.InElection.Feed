import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import { formatAbsoluteTime } from '../../utils/formatDate';
import type { ProviderTestResult } from '../../api/providerTypes';

export function TestResultPanel({ result, onClose }: { result: ProviderTestResult; onClose: () => void }) {
  return (
    <Alert severity={result.success ? 'success' : 'error'} onClose={onClose} sx={{ mt: 0.5 }}>
      <Stack gap={0.75}>
        <Typography variant="body2" fontWeight={700}>
          {result.success ? 'Test succeeded' : 'Test failed'}
        </Typography>
        <Stack direction="row" gap={1} flexWrap="wrap">
          {result.httpStatusCode !== null && <Chip size="small" variant="outlined" label={`HTTP ${result.httpStatusCode}`} />}
          <Chip size="small" variant="outlined" label={`${result.articleCount} item(s) parsed`} />
          <Chip size="small" variant="outlined" label={`${result.processingDurationMs} ms`} />
          <Chip size="small" variant="outlined" label={formatAbsoluteTime(result.fetchedAt)} />
        </Stack>
        {result.error && (
          <Typography variant="body2" color="error.main" sx={{ wordBreak: 'break-word' }}>
            {result.exceptionType ? `${result.exceptionType}: ` : ''}
            {result.error}
          </Typography>
        )}
        {result.rawResponseBody && (
          <Stack gap={0.5}>
            <Typography variant="caption" fontWeight={600} color="text.secondary">
              Raw response
            </Typography>
            <Box
              component="pre"
              sx={{
                m: 0,
                p: 1,
                maxHeight: 280,
                overflow: 'auto',
                bgcolor: 'action.hover',
                borderRadius: 1,
                fontSize: 12,
                fontFamily: 'monospace',
                whiteSpace: 'pre-wrap',
                wordBreak: 'break-word',
              }}
            >
              {result.rawResponseBody}
            </Box>
          </Stack>
        )}
      </Stack>
    </Alert>
  );
}
