#!/usr/bin/env bash

cd -- "$(dirname -- "${BASH_SOURCE[0]}")" || exit 1

if [[ -z "${HOLIDAY_MAKER_TEST_CONNECTION_STRING:-}" && -f .env.test.local ]]; then
    found=0
    while IFS= read -r line || [[ -n "$line" ]]; do
        line="${line%$'\r'}"
        case "$line" in
            ''|\#*) continue ;;
            HOLIDAY_MAKER_TEST_CONNECTION_STRING=*)
                if [[ $found -eq 1 ]]; then
                    printf 'Duplicate test connection string in .env.test.local.\n' >&2
                    exit 1
                fi
                HOLIDAY_MAKER_TEST_CONNECTION_STRING="${line#*=}"
                found=1
                ;;
            *)
                printf 'Unexpected line in .env.test.local.\n' >&2
                exit 1
                ;;
        esac
    done < .env.test.local
    if [[ $found -eq 0 ]]; then
        printf 'No test connection string found in .env.test.local.\n' >&2
        exit 1
    fi
elif [[ -z "${HOLIDAY_MAKER_TEST_CONNECTION_STRING:-}" ]]; then
    if ! read -r -s -p "MySQL test connection string: " HOLIDAY_MAKER_TEST_CONNECTION_STRING; then
        printf '\nCould not read the test connection string.\n' >&2
        exit 1
    fi
    printf '\n'
fi

HOLIDAY_MAKER_TEST_CONNECTION_STRING="${HOLIDAY_MAKER_TEST_CONNECTION_STRING%$'\r'}"
if [[ ! "$HOLIDAY_MAKER_TEST_CONNECTION_STRING" =~ [^[:space:]] ]]; then
    printf 'A test connection string is required.\n' >&2
    exit 1
fi

export HOLIDAY_MAKER_TEST_CONNECTION_STRING
dotnet test '.\HolidayMaker.Tests\HolidayMaker.Tests.csproj'
